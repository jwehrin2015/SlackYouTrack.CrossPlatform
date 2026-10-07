using Microsoft.EntityFrameworkCore;
using SlackYouTrack.CrossPlatform.Data;
using SlackYouTrack.CrossPlatform.Models;

namespace SlackYouTrack.CrossPlatform.Services;

public sealed class AttendanceRepository
{
    public async Task<IReadOnlyList<LookupOption>> GetInstructorsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = new AppDbContext();
        var instructors = await context.Instructors
            .AsNoTracking()
            .OrderBy(instructor => instructor.LastName)
            .ThenBy(instructor => instructor.FirstName)
            .Select(instructor => new { instructor.InstructorId, instructor.FirstName, instructor.LastName })
            .ToListAsync(cancellationToken);

        return instructors
            .Select(instructor => new LookupOption(
                instructor.InstructorId,
                JoinName(instructor.FirstName, instructor.LastName)))
            .ToList();
    }

    public async Task<IReadOnlyList<LookupOption>> GetCoursesAsync(
        string instructorId,
        CancellationToken cancellationToken = default)
    {
        await using var context = new AppDbContext();
        var courses = await context.Courses
            .AsNoTracking()
            .Where(course => course.InstructorId == instructorId)
            .OrderBy(course => course.CourseName)
            .Select(course => new { course.CourseId, course.CourseName })
            .ToListAsync(cancellationToken);

        return courses
            .Select(course => new LookupOption(course.CourseId, course.CourseName))
            .ToList();
    }

    public async Task<IReadOnlyList<ClassDateOption>> GetClassDatesAsync(
        string courseId,
        CancellationToken cancellationToken = default)
    {
        await using var context = new AppDbContext();
        var classes = await context.Classes
            .AsNoTracking()
            .Where(classEntity => classEntity.CourseId == courseId)
            .OrderByDescending(classEntity => classEntity.ClassDate)
            .ThenByDescending(classEntity => classEntity.ClassId)
            .Select(classEntity => new { classEntity.ClassId, classEntity.ClassDate })
            .ToListAsync(cancellationToken);

        return classes
            .Select(classEntity => new ClassDateOption(
                classEntity.ClassId.ToString(),
                classEntity.ClassDate))
            .ToList();
    }

    public async Task<AttendanceReport> GetReportAsync(
        string courseId,
        string classId,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(classId, out var parsedClassId))
        {
            throw new ArgumentException("The selected class ID is invalid.", nameof(classId));
        }

        await using var context = new AppDbContext();
        var classInfo = await context.Classes
            .AsNoTracking()
            .Where(classEntity => classEntity.ClassId == parsedClassId)
            .Select(classEntity => new
            {
                classEntity.CourseId,
                classEntity.Course.CourseName,
                classEntity.Instructor.FirstName,
                classEntity.Instructor.LastName,
                classEntity.ClassDate,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (classInfo is null)
        {
            throw new InvalidOperationException("The selected class could not be found.");
        }

        var classReport = new ClassReport(
            classInfo.CourseId,
            classInfo.CourseName,
            JoinName(classInfo.FirstName, classInfo.LastName),
            classInfo.ClassDate);

        var studentIds = await context.RegisteredClasses
            .AsNoTracking()
            .Where(registration => registration.CourseId == courseId)
            .Select(registration => registration.StudentId)
            .ToListAsync(cancellationToken);

        var students = await context.Students
            .AsNoTracking()
            .Where(student => studentIds.Contains(student.StudentId))
            .Select(student => new { student.StudentId, student.FirstName, student.LastName })
            .ToListAsync(cancellationToken);

        var attendanceRecords = await context.Attendance
            .AsNoTracking()
            .Where(attendance => attendance.ClassId == parsedClassId
                && studentIds.Contains(attendance.StudentId))
            .OrderBy(attendance => attendance.AttendanceId)
            .ToListAsync(cancellationToken);

        var attendanceByStudent = attendanceRecords
            .GroupBy(attendance => attendance.StudentId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var rows = students
            .Select(student =>
            {
                attendanceByStudent.TryGetValue(student.StudentId, out var records);
                var latestTimeIn = records?
                    .Where(record => record.TimeIn.HasValue)
                    .OrderByDescending(record => record.TimeIn)
                    .Select(record => record.TimeIn)
                    .FirstOrDefault();
                var firstRecord = records?.FirstOrDefault();

                return new AttendanceRow(
                    JoinName(student.FirstName, student.LastName),
                    firstRecord?.Attended ?? "",
                    latestTimeIn?.ToString(@"hh\:mm\:ss") ?? "",
                    records is { Count: > 0 } ? classInfo.ClassDate.ToString("yyyy-MM-dd") : "");
            })
            .OrderBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new AttendanceReport(classReport, rows);
    }

    public async Task<IReadOnlyList<LookupOption>> GetLocationsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = new AppDbContext();
        var locations = await context.Locations
            .AsNoTracking()
            .OrderBy(location => location.ClassRoom)
            .Select(location => location.ClassRoom)
            .ToListAsync(cancellationToken);

        return locations
            .Select(room => new LookupOption(room, room))
            .ToList();
    }

    public async Task<int> AddRecurringClassesAsync(
        string instructorId,
        string locationId,
        string courseId,
        TimeSpan startTime,
        TimeSpan endTime,
        DateTime startDate,
        DateTime endDate,
        IReadOnlySet<DayOfWeek> daysOfWeek,
        CancellationToken cancellationToken = default)
    {
        if (endDate.Date < startDate.Date)
        {
            throw new ArgumentException("The end date must be on or after the start date.");
        }

        if (daysOfWeek.Count == 0)
        {
            throw new ArgumentException("Select at least one weekday.");
        }

        if (endTime <= startTime)
        {
            throw new ArgumentException("The end time must be later than the start time.");
        }

        var dates = GetScheduledDates(startDate, endDate, daysOfWeek);
        if (dates.Count == 0)
        {
            throw new ArgumentException("There are no dates matching the selected weekdays in this range.");
        }

        await using var context = new AppDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        context.Classes.AddRange(dates.Select(date => new ClassEntity
        {
            ClassDate = date,
            ClassRoom = locationId,
            StartTime = startTime,
            EndTime = endTime,
            InstructorId = instructorId,
            CourseId = courseId,
        }));

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return dates.Count;
    }

    public static IReadOnlyList<DateTime> GetScheduledDates(
        DateTime startDate,
        DateTime endDate,
        IReadOnlySet<DayOfWeek> daysOfWeek)
    {
        var dates = new List<DateTime>();
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (daysOfWeek.Contains(date.DayOfWeek))
            {
                dates.Add(date);
            }
        }

        return dates;
    }

    private static string JoinName(string firstName, string lastName)
    {
        return string.Join(" ", new[] { firstName, lastName }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}

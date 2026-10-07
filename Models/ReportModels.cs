namespace SlackYouTrack.CrossPlatform.Models;

public sealed record LookupOption(string Id, string Label);

public sealed record ClassDateOption(string Id, DateTime Date, string? Prompt = null)
{
    public string Label => string.IsNullOrEmpty(Id) ? Prompt ?? "" : $"Class {Id} — {Date:yyyy-MM-dd}";
}

public sealed record ClassReport(string CourseId, string CourseName, string Professor, DateTime Date);

public sealed record AttendanceRow(string Name, string Attended, string TimeIn, string LastDateAttended);

public sealed record AttendanceReport(ClassReport Class, IReadOnlyList<AttendanceRow> Rows);

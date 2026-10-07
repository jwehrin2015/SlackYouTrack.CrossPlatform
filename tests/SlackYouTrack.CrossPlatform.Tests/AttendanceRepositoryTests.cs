using SlackYouTrack.CrossPlatform.Services;

namespace SlackYouTrack.CrossPlatform.Tests;

public sealed class AttendanceRepositoryTests
{
    [Fact]
    public void GetScheduledDates_includes_selected_weekdays_within_inclusive_range()
    {
        var days = new HashSet<DayOfWeek>
        {
            DayOfWeek.Monday,
            DayOfWeek.Wednesday,
            DayOfWeek.Friday,
        };

        var result = AttendanceRepository.GetScheduledDates(
            new DateTime(2026, 10, 4),
            new DateTime(2026, 10, 10),
            days);

        Assert.Equal(
            new[]
            {
                new DateTime(2026, 10, 5),
                new DateTime(2026, 10, 7),
                new DateTime(2026, 10, 9),
            },
            result);
    }

    [Fact]
    public void GetScheduledDates_uses_calendar_days_when_inputs_include_times()
    {
        var days = new HashSet<DayOfWeek> { DayOfWeek.Wednesday, DayOfWeek.Friday };

        var result = AttendanceRepository.GetScheduledDates(
            new DateTime(2026, 10, 7, 15, 30, 0),
            new DateTime(2026, 10, 9, 8, 0, 0),
            days);

        Assert.Equal(
            new[] { new DateTime(2026, 10, 7), new DateTime(2026, 10, 9) },
            result);
    }

    [Fact]
    public void GetScheduledDates_returns_empty_when_no_weekdays_are_selected()
    {
        var result = AttendanceRepository.GetScheduledDates(
            new DateTime(2026, 10, 5),
            new DateTime(2026, 10, 9),
            new HashSet<DayOfWeek>());

        Assert.Empty(result);
    }

    [Fact]
    public void GetScheduledDates_returns_empty_when_range_is_reversed()
    {
        var result = AttendanceRepository.GetScheduledDates(
            new DateTime(2026, 10, 9),
            new DateTime(2026, 10, 5),
            new HashSet<DayOfWeek> { DayOfWeek.Wednesday });

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddRecurringClassesAsync_rejects_end_date_before_start_date()
    {
        var repository = new AttendanceRepository();

        await Assert.ThrowsAsync<ArgumentException>(() => repository.AddRecurringClassesAsync(
            "instructor",
            "room",
            "course",
            new TimeSpan(9, 0, 0),
            new TimeSpan(10, 0, 0),
            new DateTime(2026, 10, 9),
            new DateTime(2026, 10, 5),
            new HashSet<DayOfWeek> { DayOfWeek.Wednesday }));
    }

    [Fact]
    public async Task AddRecurringClassesAsync_requires_at_least_one_weekday()
    {
        var repository = new AttendanceRepository();

        await Assert.ThrowsAsync<ArgumentException>(() => repository.AddRecurringClassesAsync(
            "instructor",
            "room",
            "course",
            new TimeSpan(9, 0, 0),
            new TimeSpan(10, 0, 0),
            new DateTime(2026, 10, 5),
            new DateTime(2026, 10, 9),
            new HashSet<DayOfWeek>()));
    }

    [Fact]
    public async Task AddRecurringClassesAsync_rejects_end_time_not_after_start_time()
    {
        var repository = new AttendanceRepository();

        await Assert.ThrowsAsync<ArgumentException>(() => repository.AddRecurringClassesAsync(
            "instructor",
            "room",
            "course",
            new TimeSpan(10, 0, 0),
            new TimeSpan(10, 0, 0),
            new DateTime(2026, 10, 5),
            new DateTime(2026, 10, 9),
            new HashSet<DayOfWeek> { DayOfWeek.Wednesday }));
    }
}

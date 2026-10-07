using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using SlackYouTrack.CrossPlatform.Data;
using SlackYouTrack.CrossPlatform.Models;
using SlackYouTrack.CrossPlatform.Services;
using System.Globalization;
using System.Net;
using System.Text;

namespace SlackYouTrack.CrossPlatform.Views;

public partial class MainWindow : Window
{
    private AttendanceReport? _currentReport;
    private bool _initialized;
    private bool _loadingLookups;
    private bool _reportLookupsLoading;
    private bool _addLookupsLoading;
    private bool _reportLookupsLoaded;
    private bool _addLookupsLoaded;
    private int _reportCourseRequest;
    private int _reportClassRequest;
    private int _addCourseRequest;

    public MainWindow()
    {
        InitializeComponent();
        StartTimePicker.SelectedTime = new TimeSpan(9, 0, 0);
        EndTimePicker.SelectedTime = new TimeSpan(10, 0, 0);
        StartDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today);
        EndDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today);
        Loaded += async (_, _) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            await DatabaseInitializer.InitializeAsync();
            DatabasePathTextBlock.Text = LocalDatabase.DatabasePath;
            _initialized = true;
            await LoadReportInstructorsAsync();
        }
        catch (Exception exception)
        {
            _initialized = true;
            ShowStatus($"Could not initialize the local database or load instructors: {exception.Message}", isError: true);
        }
    }

    private static AttendanceRepository CreateRepository() => new();

    private async void MainTabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_initialized)
        {
            return;
        }

        if (MainTabs.SelectedIndex == 0 && !_reportLookupsLoaded && !_reportLookupsLoading)
        {
            await RunUiActionAsync(LoadReportInstructorsAsync);
        }
        else if (MainTabs.SelectedIndex == 1 && !_addLookupsLoaded && !_addLookupsLoading)
        {
            await RunUiActionAsync(LoadAddClassLookupsAsync);
        }
    }

    private async Task LoadReportInstructorsAsync()
    {
        if (_reportLookupsLoading)
        {
            return;
        }

        _reportLookupsLoading = true;
        try
        {
            var instructors = await CreateRepository().GetInstructorsAsync();
            _loadingLookups = true;
            ReportInstructorCombo.ItemsSource = WithPrompt(instructors, "Select a professor");
            ReportInstructorCombo.SelectedIndex = 0;
            ReportCourseCombo.ItemsSource = null;
            ReportClassCombo.ItemsSource = null;
            ReportCourseCombo.IsEnabled = false;
            ReportClassCombo.IsEnabled = false;
            _loadingLookups = false;
            _reportLookupsLoaded = true;
            ShowStatus($"Loaded {instructors.Count} professors.");
        }
        finally
        {
            _loadingLookups = false;
            _reportLookupsLoading = false;
        }
    }

    private async Task LoadAddClassLookupsAsync()
    {
        if (_addLookupsLoading)
        {
            return;
        }

        _addLookupsLoading = true;
        try
        {
            var repository = CreateRepository();
            var instructorsTask = repository.GetInstructorsAsync();
            var locationsTask = repository.GetLocationsAsync();
            await Task.WhenAll(instructorsTask, locationsTask);

            _loadingLookups = true;
            AddInstructorCombo.ItemsSource = WithPrompt(instructorsTask.Result, "Select a professor");
            AddInstructorCombo.SelectedIndex = 0;
            AddCourseCombo.ItemsSource = null;
            AddCourseCombo.IsEnabled = false;
            LocationCombo.ItemsSource = WithPrompt(locationsTask.Result, "Select a class room");
            LocationCombo.SelectedIndex = 0;
            _loadingLookups = false;
            _addLookupsLoaded = true;
            ShowStatus($"Loaded {instructorsTask.Result.Count} professors and {locationsTask.Result.Count} class rooms.");
        }
        finally
        {
            _loadingLookups = false;
            _addLookupsLoading = false;
        }
    }

    private async void ReportInstructor_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_loadingLookups)
        {
            return;
        }

        var instructor = ReportInstructorCombo.SelectedItem as LookupOption;
        var request = ++_reportCourseRequest;
        ++_reportClassRequest;
        ReportCourseCombo.ItemsSource = null;
        ReportClassCombo.ItemsSource = null;
        ReportCourseCombo.IsEnabled = false;
        ReportClassCombo.IsEnabled = false;

        if (string.IsNullOrEmpty(instructor?.Id))
        {
            return;
        }

        await RunUiActionAsync(async () =>
        {
            var courses = await CreateRepository().GetCoursesAsync(instructor.Id);
            if (request != _reportCourseRequest)
            {
                return;
            }

            _loadingLookups = true;
            ReportCourseCombo.ItemsSource = WithPrompt(courses, "Select a course");
            ReportCourseCombo.SelectedIndex = 0;
            ReportCourseCombo.IsEnabled = true;
            _loadingLookups = false;
            ShowStatus($"Loaded {courses.Count} courses.");
        });
    }

    private async void ReportCourse_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_loadingLookups)
        {
            return;
        }

        var course = ReportCourseCombo.SelectedItem as LookupOption;
        var request = ++_reportClassRequest;
        ReportClassCombo.ItemsSource = null;
        ReportClassCombo.IsEnabled = false;

        if (string.IsNullOrEmpty(course?.Id))
        {
            return;
        }

        await RunUiActionAsync(async () =>
        {
            var classes = await CreateRepository().GetClassDatesAsync(course.Id);
            if (request != _reportClassRequest)
            {
                return;
            }

            _loadingLookups = true;
            ReportClassCombo.ItemsSource = WithPrompt(classes, "Select a class date");
            ReportClassCombo.SelectedIndex = 0;
            ReportClassCombo.IsEnabled = true;
            _loadingLookups = false;
            ShowStatus($"Loaded {classes.Count} class dates.");
        });
    }

    private async void AddInstructor_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_loadingLookups)
        {
            return;
        }

        var instructor = AddInstructorCombo.SelectedItem as LookupOption;
        var request = ++_addCourseRequest;
        AddCourseCombo.ItemsSource = null;
        AddCourseCombo.IsEnabled = false;

        if (string.IsNullOrEmpty(instructor?.Id))
        {
            return;
        }

        await RunUiActionAsync(async () =>
        {
            var courses = await CreateRepository().GetCoursesAsync(instructor.Id);
            if (request != _addCourseRequest)
            {
                return;
            }

            _loadingLookups = true;
            AddCourseCombo.ItemsSource = WithPrompt(courses, "Select a course");
            AddCourseCombo.SelectedIndex = 0;
            AddCourseCombo.IsEnabled = true;
            _loadingLookups = false;
            ShowStatus($"Loaded {courses.Count} courses.");
        });
    }

    private async void GenerateReport_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await RunUiActionAsync(async () =>
        {
            var course = RequireSelection(ReportCourseCombo, "Select a course.");
            var classDate = ReportClassCombo.SelectedItem as ClassDateOption
                ?? throw new InvalidOperationException("Select a class date.");
            if (string.IsNullOrEmpty(classDate.Id))
            {
                throw new InvalidOperationException("Select a class date.");
            }

            var report = await CreateRepository().GetReportAsync(course.Id, classDate.Id);
            _currentReport = report;
            ReportCourseText.Text = report.Class.CourseName;
            ReportProfessorText.Text = report.Class.Professor;
            ReportCourseIdText.Text = report.Class.CourseId;
            ReportDateText.Text = report.Class.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            ReportGrid.ItemsSource = report.Rows;
            ShowStatus($"Report generated with {report.Rows.Count} students.");
        });
    }

    private async void AddClasses_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await RunUiActionAsync(async () =>
        {
            var instructor = RequireSelection(AddInstructorCombo, "Select a professor.");
            var location = RequireSelection(LocationCombo, "Select a class room.");
            var course = RequireSelection(AddCourseCombo, "Select a course.");
            var startDate = StartDatePicker.SelectedDate?.Date
                ?? throw new InvalidOperationException("Select a start date.");
            var endDate = EndDatePicker.SelectedDate?.Date
                ?? throw new InvalidOperationException("Select an end date.");
            var startTime = StartTimePicker.SelectedTime
                ?? throw new InvalidOperationException("Select a start time.");
            var endTime = EndTimePicker.SelectedTime
                ?? throw new InvalidOperationException("Select an end time.");

            var days = new Dictionary<CheckBox, DayOfWeek>
            {
                [MondayCheck] = DayOfWeek.Monday,
                [TuesdayCheck] = DayOfWeek.Tuesday,
                [WednesdayCheck] = DayOfWeek.Wednesday,
                [ThursdayCheck] = DayOfWeek.Thursday,
                [FridayCheck] = DayOfWeek.Friday,
            }
            .Where(entry => entry.Key.IsChecked == true)
            .Select(entry => entry.Value)
            .ToHashSet();

            var count = await CreateRepository().AddRecurringClassesAsync(
                instructor.Id,
                location.Id,
                course.Id,
                startTime,
                endTime,
                startDate,
                endDate,
                days);

            ShowStatus($"Added {count} class{(count == 1 ? "" : "es")}.");
            _reportLookupsLoaded = false;
            if (MainTabs.SelectedIndex == 0)
            {
                await LoadReportInstructorsAsync();
            }
        });
    }

    private async void SaveCsv_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_currentReport is null)
        {
            ShowStatus("Generate a report before saving it.", isError: true);
            return;
        }

        var report = _currentReport;
        await RunUiActionAsync(async () =>
        {
            var now = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save attendance report",
                SuggestedFileName = $"SLAK-U-Track_Report_{now}.csv",
                DefaultExtension = "csv",
                FileTypeChoices = [new FilePickerFileType("CSV file") { Patterns = ["*.csv"] }],
            });

            if (file is null)
            {
                ShowStatus("CSV export cancelled.");
                return;
            }

            var csv = BuildCsv(report);
            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            await writer.WriteAsync(csv);
            ShowStatus("CSV report saved.");
        });
    }

    private async void PrintReport_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_currentReport is null)
        {
            ShowStatus("Generate a report before printing it.", isError: true);
            return;
        }

        await RunUiActionAsync(async () =>
        {
            var path = Path.Combine(Path.GetTempPath(), $"SLAK-U-Track_Print_{Guid.NewGuid():N}.html");
            await File.WriteAllTextAsync(path, BuildPrintableHtml(_currentReport), new UTF8Encoding(false));

            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }

            var opened = await (TopLevel.GetTopLevel(this)?.Launcher
                ?? throw new InvalidOperationException("Could not open the system browser."))
                .LaunchUriAsync(new Uri(path));
            if (!opened)
            {
                throw new InvalidOperationException("The system could not open the printable report.");
            }

            ShowStatus("The report opened in your browser. Use its print dialog to choose a printer.");
        });
    }

    private async Task RunUiActionAsync(Func<Task> action)
    {
        try
        {
            ShowStatus("Working…");
            await action();
        }
        catch (Exception exception)
        {
            ShowStatus(exception.Message, isError: true);
        }
    }

    private void ShowStatus(string message, bool isError = false)
    {
        StatusText.Text = message;
        StatusText.Foreground = isError ? Brushes.Firebrick : Brushes.ForestGreen;
    }

    private static IReadOnlyList<LookupOption> WithPrompt(IReadOnlyList<LookupOption> choices, string prompt)
    {
        return new[] { new LookupOption("", prompt) }.Concat(choices).ToList();
    }

    private static IReadOnlyList<ClassDateOption> WithPrompt(IReadOnlyList<ClassDateOption> choices, string prompt)
    {
        return new[] { new ClassDateOption("", DateTime.MinValue, prompt) }.Concat(choices).ToList();
    }

    private static LookupOption RequireSelection(ComboBox comboBox, string errorMessage)
    {
        var selection = comboBox.SelectedItem as LookupOption;
        return string.IsNullOrEmpty(selection?.Id)
            ? throw new InvalidOperationException(errorMessage)
            : selection;
    }

    private static string BuildCsv(AttendanceReport report)
    {
        var rows = new List<string[]>
            {
                new[] { "Course ID:", report.Class.CourseId },
                new[] { "Professor:", report.Class.Professor },
                new[] { "Course:", report.Class.CourseName },
                new[] { "Date:", report.Class.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
                new[] { "Name", "Attended", "TimeIn", "LastDateAttended" },
            };

        rows.AddRange(report.Rows.Select(row => new[]
        {
                row.Name,
                row.Attended,
                row.TimeIn,
                row.LastDateAttended,
            }));

        return string.Join(Environment.NewLine, rows.Select(row => string.Join(",", row.Select(EscapeCsvField))))
            + Environment.NewLine;
    }

    private static string EscapeCsvField(string value)
    {
        if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string BuildPrintableHtml(AttendanceReport report)
    {
        var builder = new StringBuilder("""
                <!doctype html>
                <html lang="en">
                <head>
                  <meta charset="utf-8">
                  <title>SLAK-U-Track attendance report</title>
                  <style>
                    body { font: 14px Arial, sans-serif; margin: 2rem; color: #17212b; }
                    h1 { margin-bottom: 0.5rem; }
                    .details { margin-bottom: 1.5rem; line-height: 1.7; }
                    table { width: 100%; border-collapse: collapse; }
                    th, td { border: 1px solid #8b98a5; padding: 0.5rem; text-align: left; }
                    th { background: #e8eef4; }
                    thead { display: table-header-group; }
                    @media print { body { margin: 0.5in; } }
                  </style>
                  <script>window.addEventListener('load', () => window.print());</script>
                </head>
                <body>
                  <h1>Attendance report</h1>
                  <div class="details">
                """);

        builder.Append("<strong>Course ID:</strong> ").Append(WebUtility.HtmlEncode(report.Class.CourseId)).Append("<br>");
        builder.Append("<strong>Professor:</strong> ").Append(WebUtility.HtmlEncode(report.Class.Professor)).Append("<br>");
        builder.Append("<strong>Course:</strong> ").Append(WebUtility.HtmlEncode(report.Class.CourseName)).Append("<br>");
        builder.Append("<strong>Date:</strong> ")
            .Append(WebUtility.HtmlEncode(report.Class.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            .Append("</div><table><thead><tr>");

        foreach (var heading in new[] { "Student", "Attended", "Time in", "Last date attended" })
        {
            builder.Append("<th>").Append(heading).Append("</th>");
        }

        builder.Append("</tr></thead><tbody>");
        foreach (var row in report.Rows)
        {
            builder.Append("<tr><td>").Append(WebUtility.HtmlEncode(row.Name))
                .Append("</td><td>").Append(WebUtility.HtmlEncode(row.Attended))
                .Append("</td><td>").Append(WebUtility.HtmlEncode(row.TimeIn))
                .Append("</td><td>").Append(WebUtility.HtmlEncode(row.LastDateAttended))
                .Append("</td></tr>");
        }

        return builder.Append("</tbody></table></body></html>").ToString();
    }
}
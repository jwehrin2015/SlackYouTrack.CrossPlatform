# SLAK-U-Track

SLAK-U-Track is an open-source cross-platform desktop application used for configuration and reporting of student attendance.

## Build and run

From this directory:

```sh
dotnet restore
dotnet build
dotnet run
```

In VS Code, run **Tasks: Run Build Task** to build, **Tasks: Run Task** → **run** to build and launch the app, or start debugging the **SLAK-U-Track** launch configuration.

## Tests

Run the unit tests from the project directory:

```sh
dotnet test tests/SlackYouTrack.CrossPlatform.Tests/SlackYouTrack.CrossPlatform.Tests.csproj
```

## Build packages

The GitHub Actions workflow builds self-contained Release packages for Windows, macOS, and Linux on x64 and ARM64. Packages are uploaded as workflow artifacts for pushes to `main` and pull requests targeting `main`. To publish downloadable assets, push a version tag such as `v1.0.0`; the workflow creates a GitHub Release containing each platform archive.

Windows packages are `.zip` archives; macOS and Linux packages are `.tar.gz` archives. Extract the archive and run the included application for that platform.

## Features

- Generate attendance reports by professor, course, and class date.
- Export reports as CSV using the platform's save-file dialog.
- Open a print-ready report in the system browser and use the browser's print dialog. The printable HTML is written to a user-only temporary file.
- Schedule recurring weekday classes between selected dates.
- Store attendance data in a local SQLite database managed through Entity Framework Core migrations.

## Database connection

The SQLite database is created in the current user's application-data directory under `SLAK-U-Track/attendance.db`. Entity Framework Core applies schema migrations automatically at startup. The initial database includes a small clearly labeled demo dataset; back up the database file before moving or replacing it.

The schema contains `Instructor`, `Course`, `Class`, `Location`, `Student`, `RegisteredClasses`, and `Attendance` tables.

To add a schema change, install the matching EF CLI once with `dotnet tool install --global dotnet-ef --version 10.0.12`, then create a migration with `dotnet ef migrations add MigrationName`. The app applies the new migration the next time it starts.

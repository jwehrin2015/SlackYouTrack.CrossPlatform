using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SlackYouTrack.CrossPlatform.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Instructor",
                columns: table => new
                {
                    InstructorID = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instructor", x => x.InstructorID);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    ClassRoom = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.ClassRoom);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.StudentID);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseID = table.Column<string>(type: "TEXT", nullable: false),
                    CourseName = table.Column<string>(type: "TEXT", nullable: false),
                    InstructorID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.CourseID);
                    table.ForeignKey(
                        name: "FK_Course_Instructor_InstructorID",
                        column: x => x.InstructorID,
                        principalTable: "Instructor",
                        principalColumn: "InstructorID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    ClassID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClassRoom = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    InstructorID = table.Column<string>(type: "TEXT", nullable: false),
                    CourseID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.ClassID);
                    table.ForeignKey(
                        name: "FK_Class_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Class_Instructor_InstructorID",
                        column: x => x.InstructorID,
                        principalTable: "Instructor",
                        principalColumn: "InstructorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Class_Location_ClassRoom",
                        column: x => x.ClassRoom,
                        principalTable: "Location",
                        principalColumn: "ClassRoom",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredClasses",
                columns: table => new
                {
                    StudentID = table.Column<string>(type: "TEXT", nullable: false),
                    CourseID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredClasses", x => new { x.StudentID, x.CourseID });
                    table.ForeignKey(
                        name: "FK_RegisteredClasses_Course_CourseID",
                        column: x => x.CourseID,
                        principalTable: "Course",
                        principalColumn: "CourseID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegisteredClasses_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    AttendanceID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentID = table.Column<string>(type: "TEXT", nullable: false),
                    ClassID = table.Column<int>(type: "INTEGER", nullable: false),
                    Attended = table.Column<string>(type: "TEXT", nullable: true),
                    TimeIn = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.AttendanceID);
                    table.ForeignKey(
                        name: "FK_Attendance_Class_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Class",
                        principalColumn: "ClassID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attendance_Student_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Student",
                        principalColumn: "StudentID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Instructor",
                columns: new[] { "InstructorID", "FirstName", "LastName" },
                values: new object[] { "DEMO-I-001", "Demo", "Instructor" });

            migrationBuilder.InsertData(
                table: "Location",
                column: "ClassRoom",
                value: "Demo Room A");

            migrationBuilder.InsertData(
                table: "Student",
                columns: new[] { "StudentID", "FirstName", "LastName" },
                values: new object[,]
                {
                    { "DEMO-S-001", "Alex", "Sample" },
                    { "DEMO-S-002", "Jordan", "Example" },
                    { "DEMO-S-003", "Taylor", "Demo" }
                });

            migrationBuilder.InsertData(
                table: "Course",
                columns: new[] { "CourseID", "CourseName", "InstructorID" },
                values: new object[] { "DEMO-C-001", "DEMO - Introduction to Attendance Tracking", "DEMO-I-001" });

            migrationBuilder.InsertData(
                table: "Class",
                columns: new[] { "ClassID", "ClassDate", "ClassRoom", "CourseID", "EndTime", "InstructorID", "StartTime" },
                values: new object[] { 1, new DateTime(2026, 10, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Demo Room A", "DEMO-C-001", new TimeSpan(0, 10, 0, 0, 0), "DEMO-I-001", new TimeSpan(0, 9, 0, 0, 0) });

            migrationBuilder.InsertData(
                table: "RegisteredClasses",
                columns: new[] { "CourseID", "StudentID" },
                values: new object[,]
                {
                    { "DEMO-C-001", "DEMO-S-001" },
                    { "DEMO-C-001", "DEMO-S-002" },
                    { "DEMO-C-001", "DEMO-S-003" }
                });

            migrationBuilder.InsertData(
                table: "Attendance",
                columns: new[] { "AttendanceID", "Attended", "ClassID", "StudentID", "TimeIn" },
                values: new object[,]
                {
                    { 1, "Yes", 1, "DEMO-S-001", new TimeSpan(0, 8, 57, 0, 0) },
                    { 2, "Yes", 1, "DEMO-S-002", new TimeSpan(0, 9, 3, 0, 0) },
                    { 3, "No", 1, "DEMO-S-003", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_ClassID",
                table: "Attendance",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_StudentID",
                table: "Attendance",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Class_ClassRoom",
                table: "Class",
                column: "ClassRoom");

            migrationBuilder.CreateIndex(
                name: "IX_Class_CourseID",
                table: "Class",
                column: "CourseID");

            migrationBuilder.CreateIndex(
                name: "IX_Class_InstructorID",
                table: "Class",
                column: "InstructorID");

            migrationBuilder.CreateIndex(
                name: "IX_Course_InstructorID",
                table: "Course",
                column: "InstructorID");

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredClasses_CourseID",
                table: "RegisteredClasses",
                column: "CourseID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "RegisteredClasses");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "Instructor");
        }
    }
}

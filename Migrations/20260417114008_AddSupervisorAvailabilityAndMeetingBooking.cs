using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PAS_Full_System.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorAvailabilityAndMeetingBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResearchArea",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "SupervisorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupervisorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBooked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupervisorAvailabilities_AspNetUsers_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MeetingBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AvailabilityId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupervisorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingBookings_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingBookings_AspNetUsers_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MeetingBookings_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeetingBookings_SupervisorAvailabilities_AvailabilityId",
                        column: x => x.AvailabilityId,
                        principalTable: "SupervisorAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingBookings_AvailabilityId",
                table: "MeetingBookings",
                column: "AvailabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingBookings_ProjectId",
                table: "MeetingBookings",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingBookings_StudentId",
                table: "MeetingBookings",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingBookings_SupervisorId",
                table: "MeetingBookings",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorAvailabilities_SupervisorId",
                table: "SupervisorAvailabilities",
                column: "SupervisorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingBookings");

            migrationBuilder.DropTable(
                name: "SupervisorAvailabilities");

            migrationBuilder.AddColumn<string>(
                name: "ResearchArea",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

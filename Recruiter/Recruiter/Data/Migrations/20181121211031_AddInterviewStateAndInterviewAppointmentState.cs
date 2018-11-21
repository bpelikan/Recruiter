using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddInterviewStateAndInterviewAppointmentState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InterviewAppointmentState",
                table: "InterviewAppointments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InterviewState",
                table: "ApplicationStages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InterviewAppointmentState",
                table: "InterviewAppointments");

            migrationBuilder.DropColumn(
                name: "InterviewState",
                table: "ApplicationStages");
        }
    }
}

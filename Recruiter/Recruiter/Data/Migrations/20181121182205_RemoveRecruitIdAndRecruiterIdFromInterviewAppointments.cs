using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class RemoveRecruitIdAndRecruiterIdFromInterviewAppointments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InterviewAppointments_AspNetUsers_RecruitId",
                table: "InterviewAppointments");

            migrationBuilder.DropForeignKey(
                name: "FK_InterviewAppointments_AspNetUsers_RecruiterId",
                table: "InterviewAppointments");

            migrationBuilder.DropIndex(
                name: "IX_InterviewAppointments_RecruitId",
                table: "InterviewAppointments");

            migrationBuilder.DropIndex(
                name: "IX_InterviewAppointments_RecruiterId",
                table: "InterviewAppointments");

            migrationBuilder.DropColumn(
                name: "RecruitId",
                table: "InterviewAppointments");

            migrationBuilder.DropColumn(
                name: "RecruiterId",
                table: "InterviewAppointments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RecruitId",
                table: "InterviewAppointments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecruiterId",
                table: "InterviewAppointments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAppointments_RecruitId",
                table: "InterviewAppointments",
                column: "RecruitId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAppointments_RecruiterId",
                table: "InterviewAppointments",
                column: "RecruiterId");

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewAppointments_AspNetUsers_RecruitId",
                table: "InterviewAppointments",
                column: "RecruitId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InterviewAppointments_AspNetUsers_RecruiterId",
                table: "InterviewAppointments",
                column: "RecruiterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

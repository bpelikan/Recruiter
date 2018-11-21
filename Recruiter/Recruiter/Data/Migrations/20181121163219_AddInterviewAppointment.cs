using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddInterviewAppointment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewAppointments",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    RecruitId = table.Column<string>(nullable: true),
                    RecruiterId = table.Column<string>(nullable: true),
                    InterviewId = table.Column<string>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    AcceptedByRecruit = table.Column<bool>(nullable: false),
                    AcceptedByRecruitTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewAppointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterviewAppointments_ApplicationStages_InterviewId",
                        column: x => x.InterviewId,
                        principalTable: "ApplicationStages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewAppointments_AspNetUsers_RecruitId",
                        column: x => x.RecruitId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterviewAppointments_AspNetUsers_RecruiterId",
                        column: x => x.RecruiterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAppointments_InterviewId",
                table: "InterviewAppointments",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAppointments_RecruitId",
                table: "InterviewAppointments",
                column: "RecruitId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewAppointments_RecruiterId",
                table: "InterviewAppointments",
                column: "RecruiterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewAppointments");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddHomeworkStateToHomeworkApplicationState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HomeworkState",
                table: "ApplicationStages",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendingTime",
                table: "ApplicationStages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeworkState",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "SendingTime",
                table: "ApplicationStages");
        }
    }
}

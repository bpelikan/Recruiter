using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class UpdateApplicationStages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ApplicationStages",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ApplicationStages",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "ApplicationStages",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndTime",
                table: "ApplicationStages",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "ApplicationStages",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "ApplicationStages",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "ApplicationStages");
        }
    }
}

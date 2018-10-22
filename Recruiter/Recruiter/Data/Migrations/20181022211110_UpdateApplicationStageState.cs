using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class UpdateApplicationStageState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationStageState",
                table: "ApplicationStages");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "ApplicationStages",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "ApplicationStages");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationStageState",
                table: "ApplicationStages",
                nullable: false,
                defaultValue: 0);
        }
    }
}

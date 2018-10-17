using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddRelationRequirement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationStagesRequirements_JobPositionId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.AlterColumn<string>(
                name: "JobPositionId",
                table: "ApplicationStagesRequirements",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Applications",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JobPositionId",
                table: "Applications",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirements_JobPositionId",
                table: "ApplicationStagesRequirements",
                column: "JobPositionId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApplicationStagesRequirements_JobPositionId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.AlterColumn<string>(
                name: "JobPositionId",
                table: "ApplicationStagesRequirements",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Applications",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "JobPositionId",
                table: "Applications",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirements_JobPositionId",
                table: "ApplicationStagesRequirements",
                column: "JobPositionId",
                unique: true,
                filter: "[JobPositionId] IS NOT NULL");
        }
    }
}

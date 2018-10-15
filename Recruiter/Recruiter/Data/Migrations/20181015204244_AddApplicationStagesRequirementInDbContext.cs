using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddApplicationStagesRequirementInDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStagesRequirement_JobPositions_JobPositionId",
                table: "ApplicationStagesRequirement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationStagesRequirement",
                table: "ApplicationStagesRequirement");

            migrationBuilder.RenameTable(
                name: "ApplicationStagesRequirement",
                newName: "ApplicationStagesRequirements");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationStagesRequirement_JobPositionId",
                table: "ApplicationStagesRequirements",
                newName: "IX_ApplicationStagesRequirements_JobPositionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationStagesRequirements",
                table: "ApplicationStagesRequirements",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStagesRequirements_JobPositions_JobPositionId",
                table: "ApplicationStagesRequirements",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStagesRequirements_JobPositions_JobPositionId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationStagesRequirements",
                table: "ApplicationStagesRequirements");

            migrationBuilder.RenameTable(
                name: "ApplicationStagesRequirements",
                newName: "ApplicationStagesRequirement");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationStagesRequirements_JobPositionId",
                table: "ApplicationStagesRequirement",
                newName: "IX_ApplicationStagesRequirement_JobPositionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationStagesRequirement",
                table: "ApplicationStagesRequirement",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStagesRequirement_JobPositions_JobPositionId",
                table: "ApplicationStagesRequirement",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

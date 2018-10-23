using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddDefaultResponsibleUserForApplicationStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultResponsibleForApplicatioApprovalId",
                table: "ApplicationStagesRequirements",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForApplicatioApprovalId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForApplicatioApprovalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForApplicatioApprovalId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForApplicatioApprovalId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForApplicatioApprovalId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForApplicatioApprovalId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropColumn(
                name: "DefaultResponsibleForApplicatioApprovalId",
                table: "ApplicationStagesRequirements");
        }
    }
}

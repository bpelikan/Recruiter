using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class UpdateDefaultResponsibleUserForApplicationStage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultResponsibleForHomeworkId",
                table: "ApplicationStagesRequirements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultResponsibleForInterviewId",
                table: "ApplicationStagesRequirements",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultResponsibleForPhoneCallId",
                table: "ApplicationStagesRequirements",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForHomeworkId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForHomeworkId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForInterviewId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForInterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForPhoneCallId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForPhoneCallId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForHomeworkId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForHomeworkId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForInterviewId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForInterviewId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForPhoneCallId",
                table: "ApplicationStagesRequirements",
                column: "DefaultResponsibleForPhoneCallId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForHomeworkId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForInterviewId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStagesRequirements_AspNetUsers_DefaultResponsibleForPhoneCallId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForHomeworkId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForInterviewId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStagesRequirements_DefaultResponsibleForPhoneCallId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropColumn(
                name: "DefaultResponsibleForHomeworkId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropColumn(
                name: "DefaultResponsibleForInterviewId",
                table: "ApplicationStagesRequirements");

            migrationBuilder.DropColumn(
                name: "DefaultResponsibleForPhoneCallId",
                table: "ApplicationStagesRequirements");
        }
    }
}

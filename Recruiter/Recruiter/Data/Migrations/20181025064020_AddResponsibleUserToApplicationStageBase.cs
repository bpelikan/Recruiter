using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddResponsibleUserToApplicationStageBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponsibleUserId",
                table: "ApplicationStages",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStages_ResponsibleUserId",
                table: "ApplicationStages",
                column: "ResponsibleUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStages_AspNetUsers_ResponsibleUserId",
                table: "ApplicationStages",
                column: "ResponsibleUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStages_AspNetUsers_ResponsibleUserId",
                table: "ApplicationStages");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationStages_ResponsibleUserId",
                table: "ApplicationStages");

            migrationBuilder.DropColumn(
                name: "ResponsibleUserId",
                table: "ApplicationStages");
        }
    }
}

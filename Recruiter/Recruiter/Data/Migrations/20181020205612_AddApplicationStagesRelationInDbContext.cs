using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddApplicationStagesRelationInDbContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStages_Applications_ApplicationId",
                table: "ApplicationStages");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStages_Applications_ApplicationId",
                table: "ApplicationStages",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationStages_Applications_ApplicationId",
                table: "ApplicationStages");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationStages_Applications_ApplicationId",
                table: "ApplicationStages",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

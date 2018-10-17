using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class UpdateApplicationsViewHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationsViewHistories_Applications_ApplicationId",
                table: "ApplicationsViewHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationsViewHistories_Applications_ApplicationId",
                table: "ApplicationsViewHistories",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationsViewHistories_Applications_ApplicationId",
                table: "ApplicationsViewHistories");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationsViewHistories_Applications_ApplicationId",
                table: "ApplicationsViewHistories",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

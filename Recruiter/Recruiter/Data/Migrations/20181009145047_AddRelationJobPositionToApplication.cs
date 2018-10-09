using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddRelationJobPositionToApplication : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_JobPositions_JobPositionId",
                table: "Applications");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_JobPositions_JobPositionId",
                table: "Applications",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_JobPositions_JobPositionId",
                table: "Applications");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_JobPositions_JobPositionId",
                table: "Applications",
                column: "JobPositionId",
                principalTable: "JobPositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

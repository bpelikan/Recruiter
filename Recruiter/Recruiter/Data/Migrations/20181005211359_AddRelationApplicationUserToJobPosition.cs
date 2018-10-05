using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddRelationApplicationUserToJobPosition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPositions_AspNetUsers_CreatorId",
                table: "JobPositions");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPositions_AspNetUsers_CreatorId",
                table: "JobPositions",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPositions_AspNetUsers_CreatorId",
                table: "JobPositions");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPositions_AspNetUsers_CreatorId",
                table: "JobPositions",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

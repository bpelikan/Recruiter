using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class UpdateRelationRequirement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobPositionId",
                table: "Applications",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "JobPositionId",
                table: "Applications",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}

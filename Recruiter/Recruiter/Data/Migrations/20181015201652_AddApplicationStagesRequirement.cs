using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddApplicationStagesRequirement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationStagesRequirement",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    IsApplicationApprovalRequired = table.Column<bool>(nullable: false),
                    IsPhoneCallRequired = table.Column<bool>(nullable: false),
                    IsHomeworkRequired = table.Column<bool>(nullable: false),
                    IsInterviewRequired = table.Column<bool>(nullable: false),
                    JobPositionId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStagesRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStagesRequirement_JobPositions_JobPositionId",
                        column: x => x.JobPositionId,
                        principalTable: "JobPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStagesRequirement_JobPositionId",
                table: "ApplicationStagesRequirement",
                column: "JobPositionId",
                unique: true,
                filter: "[JobPositionId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationStagesRequirement");
        }
    }
}

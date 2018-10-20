using Microsoft.EntityFrameworkCore.Migrations;

namespace Recruiter.Data.Migrations
{
    public partial class AddApplicationStages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationStages",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Accepted = table.Column<bool>(nullable: false),
                    Note = table.Column<string>(nullable: true),
                    Rate = table.Column<int>(nullable: false),
                    ApplicationId = table.Column<string>(nullable: true),
                    AcceptedById = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStages_AspNetUsers_AcceptedById",
                        column: x => x.AcceptedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationStages_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStages_AcceptedById",
                table: "ApplicationStages",
                column: "AcceptedById");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStages_ApplicationId",
                table: "ApplicationStages",
                column: "ApplicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationStages");
        }
    }
}

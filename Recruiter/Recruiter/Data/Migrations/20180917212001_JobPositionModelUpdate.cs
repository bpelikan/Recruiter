using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Recruiter.Data.Migrations
{
    public partial class JobPositionModelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "JobPositions",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "JobPositions",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "JobPositions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_JobPositions_CreatorId",
                table: "JobPositions",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPositions_AspNetUsers_CreatorId",
                table: "JobPositions",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPositions_AspNetUsers_CreatorId",
                table: "JobPositions");

            migrationBuilder.DropIndex(
                name: "IX_JobPositions_CreatorId",
                table: "JobPositions");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "JobPositions");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "JobPositions");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "JobPositions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameClubManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentUserIdAndLastActivityToComputers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentUserId",
                table: "Computers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivity",
                table: "Computers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentUserId",
                table: "Computers");

            migrationBuilder.DropColumn(
                name: "LastActivity",
                table: "Computers");
        }
    }
}

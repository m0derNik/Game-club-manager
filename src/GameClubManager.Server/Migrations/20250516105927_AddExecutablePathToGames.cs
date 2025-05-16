using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameClubManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddExecutablePathToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExecutablePath",
                table: "Games",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutablePath",
                table: "Games");
        }
    }
}

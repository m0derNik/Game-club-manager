using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameClubManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePreference_Users_UserId",
                table: "GamePreference");

            migrationBuilder.DropForeignKey(
                name: "FK_Penalty_Users_UserId",
                table: "Penalty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Penalty",
                table: "Penalty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePreference",
                table: "GamePreference");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Penalty",
                newName: "Penalties");

            migrationBuilder.RenameTable(
                name: "GamePreference",
                newName: "GamePreferences");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameIndex(
                name: "IX_Penalty_UserId",
                table: "Penalties",
                newName: "IX_Penalties_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePreference_UserId",
                table: "GamePreferences",
                newName: "IX_GamePreferences_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Penalties",
                table: "Penalties",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamePreferences",
                table: "GamePreferences",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePreferences_Users_UserId",
                table: "GamePreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Penalties_Users_UserId",
                table: "Penalties",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePreferences_Users_UserId",
                table: "GamePreferences");

            migrationBuilder.DropForeignKey(
                name: "FK_Penalties_Users_UserId",
                table: "Penalties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Penalties",
                table: "Penalties");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GamePreferences",
                table: "GamePreferences");

            migrationBuilder.RenameTable(
                name: "Penalties",
                newName: "Penalty");

            migrationBuilder.RenameTable(
                name: "GamePreferences",
                newName: "GamePreference");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "PhoneNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Penalties_UserId",
                table: "Penalty",
                newName: "IX_Penalty_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GamePreferences_UserId",
                table: "GamePreference",
                newName: "IX_GamePreference_UserId");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Penalty",
                table: "Penalty",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GamePreference",
                table: "GamePreference",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePreference_Users_UserId",
                table: "GamePreference",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Penalty_Users_UserId",
                table: "Penalty",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

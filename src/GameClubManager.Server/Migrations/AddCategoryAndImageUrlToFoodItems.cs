using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameClubManager.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryAndImageUrlToFoodItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Добавление столбца Category
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "FoodItems",
                type: "int",
                nullable: false,
                defaultValue: 0); // По умолчанию Food (0)

            // Добавление столбца ImageUrl
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "FoodItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаление столбца ImageUrl
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "FoodItems");

            // Удаление столбца Category
            migrationBuilder.DropColumn(
                name: "Category",
                table: "FoodItems");
        }
    }
} 
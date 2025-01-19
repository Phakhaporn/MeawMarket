using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace meawMarket.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePriceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "SoldCats");

            migrationBuilder.DropColumn(
                name: "Breed",
                table: "SoldCats");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "SoldCats");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "SoldCats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "SoldCats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Breed",
                table: "SoldCats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "SoldCats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "SoldCats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

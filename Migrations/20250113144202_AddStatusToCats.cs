using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace meawMarket.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToCats : Migration
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

            migrationBuilder.DropColumn(
                name: "Price",
                table: "SoldCats");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "SoldCats");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Cats",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Cats");

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

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "SoldCats",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "SoldCats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

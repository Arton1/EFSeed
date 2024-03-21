using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EFSeed.Core.Tests.Common.Migrations
{
    /// <inheritdoc />
    public partial class _2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Species",
                table: "Animals");

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Animals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AnimalClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalClasses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AnimalClasses",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 10, "Mammal" },
                    { 20, "Bird" },
                    { 30, "Reptile" },
                    { 40, "Amphibian" },
                    { 50, "Fish" },
                    { 60, "Invertebrate" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Animals_ClassId",
                table: "Animals",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Animals_AnimalClasses_ClassId",
                table: "Animals",
                column: "ClassId",
                principalTable: "AnimalClasses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Animals_AnimalClasses_ClassId",
                table: "Animals");

            migrationBuilder.DropTable(
                name: "AnimalClasses");

            migrationBuilder.DropIndex(
                name: "IX_Animals_ClassId",
                table: "Animals");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Animals");

            migrationBuilder.AddColumn<string>(
                name: "Species",
                table: "Animals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}

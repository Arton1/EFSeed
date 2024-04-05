using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFSeed.Core.Tests.Common.Migrations
{
    /// <inheritdoc />
    public partial class _4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NameLength",
                table: "Animals",
                type: "int",
                nullable: false,
                computedColumnSql: "LEN(Name)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameLength",
                table: "Animals");
        }
    }
}

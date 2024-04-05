using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFSeed.Core.Tests.Common.Migrations
{
    /// <inheritdoc />
    public partial class _5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PhoneModelId",
                table: "People",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "NameLength",
                table: "Animals",
                type: "bigint",
                nullable: false,
                computedColumnSql: "LEN(Name)",
                oldClrType: typeof(int),
                oldType: "int",
                oldComputedColumnSql: "LEN(Name)");

            migrationBuilder.CreateIndex(
                name: "IX_People_PhoneModelId",
                table: "People",
                column: "PhoneModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_People_PhoneModels_PhoneModelId",
                table: "People",
                column: "PhoneModelId",
                principalTable: "PhoneModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_People_PhoneModels_PhoneModelId",
                table: "People");

            migrationBuilder.DropIndex(
                name: "IX_People_PhoneModelId",
                table: "People");

            migrationBuilder.DropColumn(
                name: "PhoneModelId",
                table: "People");

            migrationBuilder.AlterColumn<int>(
                name: "NameLength",
                table: "Animals",
                type: "int",
                nullable: false,
                computedColumnSql: "LEN(Name)",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComputedColumnSql: "LEN(Name)");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spendly.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category_limits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    category_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    amount = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_limits", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_limits_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_limits_category_id",
                table: "category_limits",
                column: "category_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_limits");
        }
    }
}
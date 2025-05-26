using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClassificationInHeroRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "classification",
                table: "hero_records",
                type: "integer",
                nullable: false,
                // Неразмечено
                defaultValue: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "classification",
                table: "hero_records");
        }
    }
}

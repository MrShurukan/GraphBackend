using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ErVrMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "er",
                table: "hero_records",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "vr",
                table: "hero_records",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "er",
                table: "hero_records");

            migrationBuilder.DropColumn(
                name: "vr",
                table: "hero_records");
        }
    }
}

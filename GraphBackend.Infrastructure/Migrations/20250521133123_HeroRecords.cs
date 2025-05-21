using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraphBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HeroRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "hero_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    url = table.Column<string>(type: "text", nullable: false),
                    url_with_owner = table.Column<string>(type: "text", nullable: false),
                    wall_owner = table.Column<string>(type: "text", nullable: false),
                    post_author = table.Column<string>(type: "text", nullable: false),
                    date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    likes = table.Column<int>(type: "integer", nullable: false),
                    reposts = table.Column<int>(type: "integer", nullable: false),
                    comments = table.Column<int>(type: "integer", nullable: false),
                    views = table.Column<int>(type: "integer", nullable: false),
                    comment_url = table.Column<string>(type: "text", nullable: true),
                    author_name = table.Column<string>(type: "text", nullable: false),
                    subscribers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hero_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_hero_records_url",
                table: "hero_records",
                column: "url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_records");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SomeOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_hero_records_subscribers",
                table: "hero_records",
                column: "subscribers");
            
            // Включаем расширение pg_trgm (если ещё не включено)
            /*migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            // Добавляем GIN индекс с триграммами
            migrationBuilder.Sql("""
                                 CREATE INDEX idx_hero_text_trgm 
                                 ON hero_records
                                 USING GIN (text gin_trgm_ops);
                                 """);*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_hero_records_subscribers",
                table: "hero_records");
            
            /*migrationBuilder.Sql("DROP INDEX IF EXISTS idx_hero_text_trgm;");*/
        }
    }
}

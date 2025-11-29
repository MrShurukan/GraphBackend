using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraphBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HeavyOptimizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            
            migrationBuilder.Sql("""
                                 ALTER table hero_records
                                 ADD COLUMN IF NOT EXISTS text_tsv TSVECTOR;
                                 """);

            migrationBuilder.Sql("""
                                 UPDATE hero_records
                                 SET text_tsv = to_tsvector('russian', text);
                                 """);

            migrationBuilder.Sql("""
                                 CREATE INDEX IF NOT EXISTS text_tsv_index
                                 ON hero_records
                                 USING gin(text_tsv);
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE hero_records
                                 DROP COLUMN IF EXISTS text_tsv;
                                 """);
        }
    }
}

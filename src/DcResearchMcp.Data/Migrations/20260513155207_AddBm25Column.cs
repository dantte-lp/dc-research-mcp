using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DcResearchMcp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBm25Column : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // pg_tokenizer.tokenize() is STABLE (not IMMUTABLE) so a GENERATED STORED column
            // is rejected. tokenizer_catalog.create_custom_model_tokenizer_and_trigger
            // provisions (tokenizer, BM25 model, BEFORE INSERT/UPDATE trigger) that writes
            // the bm25vector into the target column on every insert/update.
            //
            // Queries: bm25v <&> to_bm25query(idx, tokenize(q, 'chunks_tokenizer')::bm25vector).
            //
            // text_analyzer_name = 'mixed_analyzer': russian_stemmer for Cyrillic, Latin
            // acronyms (Tier III, PUE, GB200 NVL72) pass through lowercased without stemming.
            // English-only standards still match because the russian stemmer leaves Latin
            // unchanged; acronyms are case-folded but otherwise preserved. Defined in
            // docker/init.sql.
            migrationBuilder.Sql("ALTER TABLE chunks ADD COLUMN IF NOT EXISTS bm25v bm25vector;");

            migrationBuilder.Sql("""
                SELECT tokenizer_catalog.create_custom_model_tokenizer_and_trigger(
                    tokenizer_name     => 'chunks_tokenizer',
                    model_name         => 'chunks_model',
                    text_analyzer_name => 'mixed_analyzer',
                    table_name         => 'chunks',
                    source_column      => '"Text"',
                    target_column      => 'bm25v'
                );
                """);

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS idx_chunks_bm25 ON chunks USING bm25 (bm25v bm25_ops);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_chunks_bm25;");

            // The trigger/model helpers don't expose a single drop — model + trigger are
            // cleaned up implicitly when the column is dropped, which cascades the TRIGGER.
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS model_chunks_model_trigger ON chunks;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS model_chunks_model_trigger_insert ON chunks;");
            migrationBuilder.Sql("DELETE FROM tokenizer_catalog.model WHERE name = 'chunks_model';");
            migrationBuilder.Sql("SELECT tokenizer_catalog.drop_tokenizer('chunks_tokenizer');");
            migrationBuilder.Sql("ALTER TABLE chunks DROP COLUMN IF EXISTS bm25v;");
        }
    }
}

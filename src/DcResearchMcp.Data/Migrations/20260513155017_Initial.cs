using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace DcResearchMcp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_tokenizer", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:vchord", ",,")
                .Annotation("Npgsql:PostgresExtension:vchord_bm25", ",,")
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.CreateTable(
                name: "chunks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContentId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    SourceFile = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Origin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                    LlmSource = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    PromptId = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    H1 = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    H2 = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    H3 = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SizeChars = table.Column<int>(type: "integer", nullable: false),
                    Embedding = table.Column<HalfVector>(type: "halfvec(384)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chunks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chunks_Category",
                table: "chunks",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_chunks_ContentId",
                table: "chunks",
                column: "ContentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chunks_Embedding",
                table: "chunks",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "halfvec_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 200)
                .Annotation("Npgsql:StorageParameter:m", 16);

            migrationBuilder.CreateIndex(
                name: "IX_chunks_Origin",
                table: "chunks",
                column: "Origin");

            migrationBuilder.CreateIndex(
                name: "IX_chunks_PromptId",
                table: "chunks",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_chunks_SourceFile",
                table: "chunks",
                column: "SourceFile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chunks");
        }
    }
}

using Pgvector;

namespace DcResearchMcp.Data.Catalog;

/// <summary>
/// EF Core entity mirroring <see cref="DcResearchMcp.Core.Catalog.Chunk"/>.
/// Persisted in the <c>chunks</c> table; the <c>vchord_bm25</c> column is
/// attached via raw-SQL migration (the tokenizer trigger is created via
/// <c>tokenizer_catalog.create_custom_model_tokenizer_and_trigger</c>).
/// </summary>
public sealed class ChunkEntity
{
    /// <summary>Identity primary key (bigint).</summary>
    public long Id { get; init; }

    /// <summary>Content-derived stable identifier (SHA-256-based 40-char hash).</summary>
    public required string ContentId { get; init; }

    /// <summary>Raw chunk text (markdown).</summary>
    public required string Text { get; init; }

    /// <summary>Source file path relative to the corpus root, normalised to '/'.</summary>
    public required string SourceFile { get; init; }

    /// <summary>Origin marker (research / tech-spec / overview / standard / vendor).</summary>
    public required string Origin { get; init; }

    /// <summary>Discipline / topic facet (see Core.Catalog.KnownCategories).</summary>
    public required string Category { get; init; }

    /// <summary>Zero-based index within the parent document.</summary>
    public required int ChunkIndex { get; init; }

    /// <summary>"claude" / "openai" — for research deliverables; null otherwise.</summary>
    public string? LlmSource { get; init; }

    /// <summary>Short prompt identifier (A1, 06-B, C-FU-2, ...) — for research deliverables.</summary>
    public string? PromptId { get; init; }

    /// <summary>Nearest H1 at chunk start, if any.</summary>
    public string? H1 { get; init; }

    /// <summary>Nearest H2 at chunk start, if any.</summary>
    public string? H2 { get; init; }

    /// <summary>Nearest H3 at chunk start, if any.</summary>
    public string? H3 { get; init; }

    /// <summary>Chunk length in characters (denormalised for faster faceting).</summary>
    public int SizeChars { get; init; }

    /// <summary>
    /// Dense embedding produced by <c>intfloat/multilingual-e5-small</c> (dim 384, halfvec).
    /// Populated by the ingest pipeline; null on parent/skip rows.
    /// </summary>
    public HalfVector? Embedding { get; init; }

    /// <summary>Server-side timestamp of insertion (UTC) — default <c>now()</c>.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

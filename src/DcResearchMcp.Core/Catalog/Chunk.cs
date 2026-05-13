namespace DcResearchMcp.Core.Catalog;

/// <summary>
/// One indexed fragment of a document — what the search returns to clients.
/// The <see cref="Id"/> is content-stable: regenerating from the same
/// (path, index, text) yields the same id.
/// </summary>
/// <param name="Id">SHA-256-derived 40-char identifier (stable across re-runs).</param>
/// <param name="Text">Raw chunk text (markdown).</param>
/// <param name="Document">Static metadata of the parent document.</param>
/// <param name="ChunkIndex">Zero-based index within the parent document.</param>
/// <param name="H1">Nearest H1 heading at the chunk start, if any.</param>
/// <param name="H2">Nearest H2 heading at the chunk start, if any.</param>
/// <param name="H3">Nearest H3 heading at the chunk start, if any.</param>
public sealed record Chunk(
    string Id,
    string Text,
    DocumentMetadata Document,
    int ChunkIndex,
    string? H1,
    string? H2,
    string? H3);

namespace DcResearchMcp.Core.Chunking;

/// <summary>
/// Pre-DB chunk produced by <see cref="IChunker"/>. The ingest pipeline turns
/// each draft into a persisted <see cref="DcResearchMcp.Core.Catalog.Chunk"/>
/// once a content-stable id is computed.
/// </summary>
/// <param name="Text">Chunk text (markdown).</param>
/// <param name="ChunkIndex">Zero-based index within the parent document.</param>
/// <param name="H1">Nearest H1 heading at chunk start.</param>
/// <param name="H2">Nearest H2 heading at chunk start.</param>
/// <param name="H3">Nearest H3 heading at chunk start.</param>
/// <param name="SizeChars">Length of <paramref name="Text"/> in chars (denormalised).</param>
public sealed record ChunkDraft(
    string Text,
    int ChunkIndex,
    string? H1,
    string? H2,
    string? H3,
    int SizeChars);

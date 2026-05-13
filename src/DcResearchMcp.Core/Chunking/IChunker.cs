namespace DcResearchMcp.Core.Chunking;

/// <summary>Splits a markdown document into header-aware, size-bounded chunks.</summary>
public interface IChunker
{
    /// <summary>
    /// Produce chunk drafts for <paramref name="markdown"/>. Implementations preserve
    /// markdown formatting inside chunks; they do not strip headings or normalise text.
    /// </summary>
    IReadOnlyList<ChunkDraft> Chunk(string markdown);
}

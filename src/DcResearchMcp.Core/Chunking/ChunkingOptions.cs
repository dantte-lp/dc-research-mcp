namespace DcResearchMcp.Core.Chunking;

/// <summary>Tunable parameters for the markdown chunker. Defaults match the corpus baseline.</summary>
public sealed record ChunkingOptions
{
    /// <summary>Target chunk size in characters (header-aware sections are split to this size).</summary>
    public int TargetChars { get; init; } = 2000;

    /// <summary>Overlap between adjacent chunks (carries tail of previous chunk into the next one).</summary>
    public int OverlapChars { get; init; } = 320;

    /// <summary>Minimum chunk size; smaller tails are dropped.</summary>
    public int MinChunkChars { get; init; } = 200;
}

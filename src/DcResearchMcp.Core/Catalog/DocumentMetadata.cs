namespace DcResearchMcp.Core.Catalog;

/// <summary>
/// Static metadata extracted from a source-file path and filename — derivable from
/// the file alone (no content inspection needed beyond filesystem stat).
/// </summary>
/// <param name="SourceFile">Path relative to the corpus source root, normalised to '/'.</param>
/// <param name="Origin">High-level provenance.</param>
/// <param name="Category">One of <see cref="KnownCategories"/> values or <c>"general"</c>.</param>
/// <param name="LlmSource"><c>"claude"</c> / <c>"openai"</c> for research deliverables; <c>null</c> otherwise.</param>
/// <param name="PromptId">Short prompt identifier (e.g. <c>A1</c>, <c>06-B</c>, <c>C-FU-2</c>) when derivable; <c>null</c> otherwise.</param>
/// <param name="SizeBytes">File size on disk at ingest time.</param>
/// <param name="LastModified">File mtime at ingest time (UTC).</param>
public sealed record DocumentMetadata(
    string SourceFile,
    Origin Origin,
    string Category,
    string? LlmSource,
    string? PromptId,
    long SizeBytes,
    DateTimeOffset LastModified);

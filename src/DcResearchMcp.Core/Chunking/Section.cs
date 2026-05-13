namespace DcResearchMcp.Core.Chunking;

/// <summary>
/// A markdown section as seen by the chunker — the body of text between one
/// heading and the next of equal-or-higher level, tagged with the deepest
/// heading triple (H1/H2/H3) visible at the section start.
/// </summary>
/// <param name="H1">Nearest H1 heading at section start, if any.</param>
/// <param name="H2">Nearest H2 heading at section start, if any.</param>
/// <param name="H3">Nearest H3 heading at section start, if any.</param>
/// <param name="Body">Section body (may include the heading line itself).</param>
public sealed record Section(string? H1, string? H2, string? H3, string Body);

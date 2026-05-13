namespace DcResearchMcp.Core.Catalog;

/// <summary>
/// Top-level provenance of a corpus document — where the raw file came from
/// before chunking. Mirrors the source roots configured in the indexer.
/// </summary>
public enum Origin
{
    /// <summary>Research findings: <c>30-findings/**/*.md</c> (Claude + OpenAI deep research).</summary>
    Research = 0,

    /// <summary>Technical specification: <c>60-tech-spec/**/*.md</c>.</summary>
    TechSpec = 1,

    /// <summary>Project overview docs: <c>00-overview/**/*.md</c>.</summary>
    Overview = 2,

    /// <summary>Converted commercial standards: <c>assets/02-standards/converted/**</c>.</summary>
    Standard = 3,

    /// <summary>Vendor materials: <c>assets/01-from-uzum-tz/converted/**</c>.</summary>
    Vendor = 4,
}

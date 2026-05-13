namespace DcResearchMcp.Core;

/// <summary>
/// Marker type used by tests for assembly discovery (typeof(AssemblyMarker).Assembly).
/// Domain models, abstractions and shared options land here in Sprint 1.
/// </summary>
public static class AssemblyMarker
{
    /// <summary>Assembly identifier — kept stable for diagnostics.</summary>
    public const string AssemblyName = "DcResearchMcp.Core";
}

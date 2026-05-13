namespace DcResearchMcp.Data;

/// <summary>
/// Marker type used by tests for assembly discovery (typeof(AssemblyMarker).Assembly).
/// EF Core DbContext, entities, migrations and COPY loader land here in Sprint 1.
/// </summary>
public static class AssemblyMarker
{
    /// <summary>Assembly identifier — kept stable for diagnostics.</summary>
    public const string AssemblyName = "DcResearchMcp.Data";
}

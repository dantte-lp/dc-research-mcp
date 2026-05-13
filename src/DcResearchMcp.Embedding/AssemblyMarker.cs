namespace DcResearchMcp.Embedding;

/// <summary>
/// Marker type used by tests for assembly discovery (typeof(AssemblyMarker).Assembly).
/// ONNX Runtime embedder, tokenizer and pooling land here in Sprint 2.
/// </summary>
public static class AssemblyMarker
{
    /// <summary>Assembly identifier — kept stable for diagnostics.</summary>
    public const string AssemblyName = "DcResearchMcp.Embedding";
}

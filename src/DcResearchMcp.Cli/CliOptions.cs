namespace DcResearchMcp.Cli;

/// <summary>
/// Runtime configuration for the CLI, sourced from (in order of precedence)
/// command-line args → environment (<c>DC_RESEARCH_PG_CONN</c>, <c>DC_RESEARCH_SOURCE_DIR</c>) →
/// <c>appsettings.json</c> → built-in defaults (local Podman compose stack).
/// </summary>
public sealed record CliOptions
{
    // The local Podman compose stack ships with dev-only credentials matching docker/compose.yaml.
    // Production deployments override via the DC_RESEARCH_PG_CONN environment variable.
    // SECURITY.md documents this as a known-accepted risk.
#pragma warning disable S2068 // hard-coded local-dev creds match docker/compose.yaml
    public string ConnectionString { get; init; }
        = "Host=localhost;Port=5435;Database=dc_research;Username=dc_research;Password=dc_research";
#pragma warning restore S2068

    /// <summary>Root directory of the research corpus.</summary>
    public string SourceDir { get; init; } = "C:/SHARE/dc_greenfield_research";
}

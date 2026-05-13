using DcResearchMcp.Data;
using Microsoft.EntityFrameworkCore;

namespace DcResearchMcp.Cli;

/// <summary>
/// Builds the <see cref="DcResearchDbContext"/> for ad-hoc CLI commands.
/// Resolves the connection string from (1) CLI option, (2) <c>DC_RESEARCH_PG_CONN</c>
/// environment variable, (3) the <see cref="CliOptions"/> default.
/// </summary>
public static class HostFactory
{
    public static string ResolveConnectionString(string? cliValue)
    {
        if (!string.IsNullOrWhiteSpace(cliValue))
        {
            return cliValue;
        }
        var env = Environment.GetEnvironmentVariable("DC_RESEARCH_PG_CONN");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }
        return new CliOptions().ConnectionString;
    }

    public static string ResolveSourceDir(string? cliValue)
    {
        if (!string.IsNullOrWhiteSpace(cliValue))
        {
            return cliValue;
        }
        var env = Environment.GetEnvironmentVariable("DC_RESEARCH_SOURCE_DIR");
        if (!string.IsNullOrWhiteSpace(env))
        {
            return env;
        }
        return new CliOptions().SourceDir;
    }

    public static DcResearchDbContext CreateDbContext(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var ds = DataSourceFactory.Build(connectionString);
        var opt = new DbContextOptionsBuilder<DcResearchDbContext>()
            .UseNpgsql(ds, o => o.UseVector())
            .Options;
        return new DcResearchDbContext(opt);
    }
}

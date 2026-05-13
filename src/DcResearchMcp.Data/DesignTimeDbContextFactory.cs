using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DcResearchMcp.Data;

/// <summary>
/// Design-time factory used by <c>dotnet ef migrations add</c> / <c>database update</c>.
/// At runtime the Server/Cli host wires DI explicitly; this factory is only consulted
/// by the EF Core tooling.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DcResearchDbContext>
{
    public DcResearchDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("DC_RESEARCH_PG_CONN")
            ?? "Host=localhost;Port=5435;Database=dc_research;Username=dc_research;Password=dc_research";
        var ds = DataSourceFactory.Build(cs);
        var opt = new DbContextOptionsBuilder<DcResearchDbContext>()
            .UseNpgsql(ds, o => o.UseVector())
            .Options;
        return new DcResearchDbContext(opt);
    }
}

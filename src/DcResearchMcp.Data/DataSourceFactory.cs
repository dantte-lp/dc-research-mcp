using Npgsql;

namespace DcResearchMcp.Data;

/// <summary>
/// Constructs an <see cref="NpgsqlDataSource"/> wired up for Pgvector (vector / halfvec).
/// The dc-research corpus uses halfvec(384) (multilingual-e5-small) by default;
/// the data-source is shared across DbContext instances.
/// </summary>
public static class DataSourceFactory
{
    /// <summary>Build a Pgvector-aware data source for the given Npgsql connection string.</summary>
    public static NpgsqlDataSource Build(string connectionString)
    {
        var b = new NpgsqlDataSourceBuilder(connectionString);
        b.UseVector();
        return b.Build();
    }
}

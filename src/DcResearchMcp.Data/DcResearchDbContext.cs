using DcResearchMcp.Data.Catalog;
using Microsoft.EntityFrameworkCore;

namespace DcResearchMcp.Data;

/// <summary>
/// EF Core context for the DC research corpus.
/// The dense embedding column (vector(384)) and the <c>vchord_bm25</c> bm25v
/// column are attached via raw-SQL migration (Sprint 1 add-on) because the
/// Pgvector.EFCore + vchord_bm25 mappings need POCO-side and SQL-side wiring
/// that EF migrations cannot infer from attributes alone.
/// </summary>
public sealed class DcResearchDbContext(DbContextOptions<DcResearchDbContext> options) : DbContext(options)
{
    /// <summary>The chunk corpus — one row per indexed fragment.</summary>
    public DbSet<ChunkEntity> Chunks => Set<ChunkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("vchord");
        modelBuilder.HasPostgresExtension("vchord_bm25");
        modelBuilder.HasPostgresExtension("pg_tokenizer");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<ChunkEntity>(b =>
        {
            b.ToTable("chunks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityByDefaultColumn();
            b.Property(x => x.ContentId).HasMaxLength(64).IsRequired();
            b.Property(x => x.Text).IsRequired();
            b.Property(x => x.SourceFile).HasMaxLength(512).IsRequired();
            b.Property(x => x.Origin).HasMaxLength(32).IsRequired();
            b.Property(x => x.Category).HasMaxLength(64).IsRequired();
            b.Property(x => x.H1).HasMaxLength(512);
            b.Property(x => x.H2).HasMaxLength(512);
            b.Property(x => x.H3).HasMaxLength(512);
            b.Property(x => x.LlmSource).HasMaxLength(16);
            b.Property(x => x.PromptId).HasMaxLength(32);

            b.HasIndex(x => x.ContentId).IsUnique();
            b.HasIndex(x => x.SourceFile);
            b.HasIndex(x => x.Category);
            b.HasIndex(x => x.Origin);
            b.HasIndex(x => x.PromptId);
        });
    }
}

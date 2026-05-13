using System.CommandLine;
using DcResearchMcp.Data.Catalog;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace DcResearchMcp.Cli.Commands;

/// <summary>
/// <c>info</c> — corpus statistics: chunks total, file count, and facet breakdowns
/// by <see cref="ChunkEntity.Origin"/> and <see cref="ChunkEntity.Category"/>.
/// </summary>
public static class InfoCommand
{
    public static Command Build(Option<string?> connOption)
    {
        var cmd = new Command("info", "Corpus statistics (chunks, files, origin / category breakdown).");
        cmd.SetAction(async (parse, ct) =>
        {
            var cs = HostFactory.ResolveConnectionString(parse.GetValue(connOption));
            await using var ctx = HostFactory.CreateDbContext(cs);

            AnsiConsole.MarkupLine($"[dim]target:[/] [cyan]{DbCommands.Mask(cs)}[/]");

            int total;
            int filesCount;
            List<(string Key, int Count)> byOrigin;
            List<(string Key, int Count)> byCategory;
            try
            {
                total = await ctx.Chunks.CountAsync(ct).ConfigureAwait(false);
                filesCount = await ctx.Chunks
                    .Select(c => c.SourceFile)
                    .Distinct()
                    .CountAsync(ct)
                    .ConfigureAwait(false);
                byOrigin = await ctx.Chunks
                    .GroupBy(c => c.Origin)
                    .Select(g => new { Key = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => ValueTuple.Create(x.Key, x.Count))
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
                byCategory = await ctx.Chunks
                    .GroupBy(c => c.Category)
                    .Select(g => new { Key = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Select(x => ValueTuple.Create(x.Key, x.Count))
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
            catch (Npgsql.PostgresException ex) when (string.Equals(ex.SqlState, "42P01", StringComparison.Ordinal))
            {
                AnsiConsole.MarkupLine("[red]table 'chunks' is missing — run [bold]db migrate[/] first[/]");
                return 1;
            }

            AnsiConsole.MarkupLine($"[bold]Total chunks:[/] {total:N0}    [bold]Distinct files:[/] {filesCount:N0}");

            var originTable = new Table().Title("[bold]By origin[/]").AddColumns("origin", "chunks");
            foreach (var (k, v) in byOrigin)
            {
                originTable.AddRow(k, v.ToString("N0", System.Globalization.CultureInfo.InvariantCulture));
            }
            AnsiConsole.Write(originTable);

            var categoryTable = new Table().Title("[bold]By category (top 20)[/]").AddColumns("category", "chunks");
            foreach (var (k, v) in byCategory.Take(20))
            {
                categoryTable.AddRow(k, v.ToString("N0", System.Globalization.CultureInfo.InvariantCulture));
            }
            AnsiConsole.Write(categoryTable);

            return 0;
        });
        return cmd;
    }
}

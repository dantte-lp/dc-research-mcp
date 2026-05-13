using System.CommandLine;
using Spectre.Console;

namespace DcResearchMcp.Cli.Commands;

/// <summary>
/// <c>ingest</c> — walk the corpus source directory, chunk every <c>.md</c> file
/// with <see cref="DcResearchMcp.Core.Chunking.MarkdownChunker"/>, derive metadata
/// (Origin/Category/PromptId/LlmSource) from the path, and bulk-load chunks via
/// Npgsql COPY. Reused by <c>reindex</c> after a <c>db drop + migrate</c>.
/// </summary>
public static class IngestCommand
{
    public static Command Build(Option<string?> connOption, Option<string?> sourceOption)
    {
        var cmd = new Command("ingest", "Ingest the corpus into the dc-research database.");
        var dryRun = new Option<bool>("--dry-run") { Description = "Walk + chunk only; do not write to the database." };
        cmd.Add(dryRun);

        cmd.SetAction((parse, ct) =>
        {
            _ = parse.GetValue(connOption);
            var src = HostFactory.ResolveSourceDir(parse.GetValue(sourceOption));
            var isDryRun = parse.GetValue(dryRun);

            AnsiConsole.MarkupLine($"[dim]source:[/] [cyan]{src}[/]   dry-run = {isDryRun}");
            AnsiConsole.MarkupLine("[yellow]ingest pipeline lands in Sprint 3b (file enumeration + Npgsql COPY).[/]");
            AnsiConsole.MarkupLine("[dim]MarkdownChunker (Core) and ChunkEntity (Data) are ready; this verb is wired but not implemented.[/]");

            return Task.FromResult(0);
        });
        return cmd;
    }
}

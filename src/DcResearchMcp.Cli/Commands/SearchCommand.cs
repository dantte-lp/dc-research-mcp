using System.CommandLine;
using Spectre.Console;

namespace DcResearchMcp.Cli.Commands;

/// <summary>
/// <c>search</c> — BM25 query over the <c>chunks</c> table via
/// <c>tokenizer_catalog</c> + <c>vchord_bm25</c>. Hybrid BM25 ⊕ dense + RRF
/// is wired after the dense embedder lands (Sprint 2 add-on + Sprint 4).
/// </summary>
public static class SearchCommand
{
    public static Command Build(Option<string?> connOption)
    {
        var cmd = new Command("search", "Run a query against the corpus (BM25 in Sprint 3c, hybrid later).");

        var query = new Argument<string>("query") { Description = "Search query (free text)." };
        var k = new Option<int>("--top-k", "-k") { DefaultValueFactory = _ => 10, Description = "Number of results." };
        var category = new Option<string?>("--category") { Description = "Filter by category facet." };
        cmd.Add(query);
        cmd.Add(k);
        cmd.Add(category);

        cmd.SetAction((parse, ct) =>
        {
            _ = parse.GetValue(connOption);
            var q = parse.GetValue(query) ?? string.Empty;
            var topK = parse.GetValue(k);
            var cat = parse.GetValue(category);

            AnsiConsole.MarkupLine($"[dim]query:[/] [cyan]{q}[/]   top-k = {topK}   category = {cat ?? "(any)"}");
            AnsiConsole.MarkupLine("[yellow]search execution lands in Sprint 3c (vchord_bm25 query + table render).[/]");

            return Task.FromResult(0);
        });
        return cmd;
    }
}

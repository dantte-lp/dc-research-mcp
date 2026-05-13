using System.CommandLine;

using DcResearchMcp.Cli.Commands;

var root = new RootCommand("dc-research-mcp — CLI for the DC Greenfield Tashkent research corpus");

var connOption = new Option<string?>("--conn")
{
    Description = "Npgsql connection string (overrides DC_RESEARCH_PG_CONN / appsettings.json).",
};
var sourceOption = new Option<string?>("--source-dir")
{
    Description = "Corpus source root (overrides DC_RESEARCH_SOURCE_DIR / appsettings.json).",
};
root.Add(connOption);
root.Add(sourceOption);

root.Subcommands.Add(DbCommands.Build(connOption));
root.Subcommands.Add(InfoCommand.Build(connOption));
root.Subcommands.Add(IngestCommand.Build(connOption, sourceOption));
root.Subcommands.Add(SearchCommand.Build(connOption));

return await root.Parse(args).InvokeAsync().ConfigureAwait(false);

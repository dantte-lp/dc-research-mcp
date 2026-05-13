using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace DcResearchMcp.Cli.Commands;

/// <summary>
/// <c>db</c> subcommands: <c>migrate</c> (apply EF migrations) and <c>drop</c>
/// (delete the database — guarded by an interactive confirmation).
/// </summary>
public static class DbCommands
{
    public static Command Build(Option<string?> connOption)
    {
        var db = new Command("db", "Database administration (migrate / drop).");

        var migrate = new Command("migrate", "Apply pending EF Core migrations to the target database.");
        migrate.SetAction(async (parse, ct) =>
        {
            var cs = HostFactory.ResolveConnectionString(parse.GetValue(connOption));
            await using var ctx = HostFactory.CreateDbContext(cs);

            AnsiConsole.MarkupLine($"[dim]target:[/] [cyan]{Mask(cs)}[/]");
            AnsiConsole.MarkupLine("[bold]applying migrations…[/]");
            await ctx.Database.MigrateAsync(ct).ConfigureAwait(false);
            AnsiConsole.MarkupLine("[green]✓ migrations up to date[/]");
            return 0;
        });

        var drop = new Command("drop", "DROP the entire database. Asks for confirmation.");
        var yesOption = new Option<bool>("--yes", "-y")
        {
            Description = "Skip the confirmation prompt (use only in scripts).",
        };
        drop.Add(yesOption);
        drop.SetAction(async (parse, ct) =>
        {
            var cs = HostFactory.ResolveConnectionString(parse.GetValue(connOption));
            var yes = parse.GetValue(yesOption);
            await using var ctx = HostFactory.CreateDbContext(cs);

            AnsiConsole.MarkupLine($"[red]about to DROP database for:[/] [yellow]{Mask(cs)}[/]");
            if (!yes && !AnsiConsole.Confirm("Drop the database? This is irreversible.", defaultValue: false))
            {
                AnsiConsole.MarkupLine("[dim]aborted[/]");
                return 1;
            }
            await ctx.Database.EnsureDeletedAsync(ct).ConfigureAwait(false);
            AnsiConsole.MarkupLine("[green]✓ database dropped[/]");
            return 0;
        });

        db.Subcommands.Add(migrate);
        db.Subcommands.Add(drop);
        return db;
    }

    /// <summary>Mask the password segment of a Npgsql connection string for log output.</summary>
    internal static string Mask(string connectionString)
    {
        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
        {
            var kv = parts[i].Split('=', 2);
            if (kv.Length == 2 && string.Equals(kv[0].Trim(), "Password", StringComparison.OrdinalIgnoreCase))
            {
                parts[i] = $"{kv[0]}=***";
            }
        }
        return string.Join(';', parts);
    }
}

using Agile.Core;
using Agile.Core.Auth;
using Agile.Core.Clients;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;

namespace Agile.Cli.Commands;

public class ValidateTokenCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        AnsiConsole.MarkupLine("[bold]GitHub Token Validation[/]");
        AnsiConsole.WriteLine();

        // Step 1: Resolve token
        string token;
        try
        {
            token = await ResolveTokenAsync();
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] Token resolution: {ex.Message}");
            AnsiConsole.MarkupLine("[dim]Run 'gh auth login' or set GITHUB_TOKEN.[/]");
            return 1;
        }

        var masked = token.Length > 8 ? token[..4] + "****" + token[^4..] : "****";
        AnsiConsole.MarkupLine($"[green]✓[/] Token resolved: [dim]{masked}[/]");

        // Step 2: Validate token via GitHub API
        var validator = new TokenValidator();
        TokenValidationResult result;
        try
        {
            result = await validator.ValidateAsync(token);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] GitHub API check failed: {ex.Message}");
            return 1;
        }

        if (!result.IsValid)
        {
            AnsiConsole.MarkupLine($"[red]✗[/] GitHub API: {result.Error}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[green]✓[/] GitHub API: token valid");
        if (result.Scopes.Length > 0)
            AnsiConsole.MarkupLine($"   Scopes: [dim]{string.Join(", ", result.Scopes)}[/]");

        // Step 3: Copilot exchange result (from validator)
        if (result.Error != null)
            AnsiConsole.MarkupLine($"[yellow]⚠[/] Copilot: {result.Error}");
        else
            AnsiConsole.MarkupLine("[green]✓[/] Copilot session token: exchange successful");

        // Step 4: GitHub Models endpoint probe
        AnsiConsole.Markup("[dim]Testing GitHub Models endpoint...[/] ");
        int exitCode = 0;
        try
        {
            var client = new GitHubModelsClient(ghTokenProvider: () => Task.FromResult(token));
            var response = await client.CompleteAsync("Reply with exactly: OK");
            AnsiConsole.MarkupLine("[green]✓[/]");
            AnsiConsole.MarkupLine($"   Response: [dim]{response.Trim()[..Math.Min(60, response.Trim().Length)]}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]✗[/]");
            AnsiConsole.MarkupLine($"   GitHub Models error: [dim]{ex.Message[..Math.Min(120, ex.Message.Length)]}[/]");
            exitCode = 1;
        }

        AnsiConsole.WriteLine();
        return result.Error != null ? 1 : exitCode;
    }

    private async Task<string> ResolveTokenAsync()
    {
        var envToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(envToken)) return envToken;

        var startInfo = new ProcessStartInfo
        {
            FileName = "gh",
            Arguments = "auth token",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) throw new InvalidOperationException("Could not start GitHub CLI (gh).");

        var token = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(token))
            return token.Trim();

        var settings = Core.SettingsManager.Load();
        if (!string.IsNullOrEmpty(settings.GithubToken))
            return settings.GithubToken;

        throw new InvalidOperationException("No GitHub token found. Set GITHUB_TOKEN or run 'gh auth login'.");
    }
}

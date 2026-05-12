using Agile.Core.Clients;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Agile.Cli.Commands;

public class ModelsSettings : CommandSettings
{
    [CommandOption("--config <PATH>")]
    [Description("Path to config.yaml (default: searches common locations)")]
    public string? Config { get; set; }
}

public class ModelsCommand : AsyncCommand<ModelsSettings>
{
    private static readonly string[] ConfigSearchPaths =
    [
        "config.yaml",
        "templates/config.yaml",
        "../templates/config.yaml",
        "project-agile/templates/config.yaml",
    ];

    public override async Task<int> ExecuteAsync(CommandContext context, ModelsSettings settings)
    {
        var configPath = settings.Config ?? ConfigSearchPaths.FirstOrDefault(File.Exists);
        if (configPath == null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] config.yaml not found. Use --config <path>.");
            return 1;
        }

        var yaml = await File.ReadAllTextAsync(configPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var config = deserializer.Deserialize<ConfigFile>(yaml);
        var models = config.Models?.TestModels ?? [];

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Name")
            .AddColumn("Provider")
            .AddColumn("Model ID")
            .AddColumn("Endpoint")
            .AddColumn("Status");

        await AnsiConsole.Status().StartAsync("Pinging endpoints...", async ctx =>
        {
            foreach (var m in models)
            {
                ctx.Status($"Pinging {m.Name}...");
                string status = await PingAsync(m);
                table.AddRow(
                    m.Name ?? "",
                    m.Provider ?? "",
                    m.ModelId ?? "",
                    m.Endpoint ?? "",
                    status);
            }
        });

        AnsiConsole.Write(table);
        return 0;
    }

    private static async Task<string> PingAsync(ModelEntry m)
    {
        try
        {
            var apiKey = m.ApiKeyEnv != null ? Environment.GetEnvironmentVariable(m.ApiKeyEnv) : null;
            if (string.IsNullOrWhiteSpace(apiKey))
                return "[yellow]⚠ no API key[/]";

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            var resp = await http.GetAsync(m.Endpoint);
            return resp.IsSuccessStatusCode || (int)resp.StatusCode < 500
                ? "[green]✓ reachable[/]"
                : $"[red]✗ HTTP {(int)resp.StatusCode}[/]";
        }
        catch
        {
            return "[red]✗ failed[/]";
        }
    }
}

internal class ConfigFile
{
    public ModelsSection? Models { get; set; }
}

internal class ModelsSection
{
    public List<ModelEntry> TestModels { get; set; } = new();
}

internal class ModelEntry
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Provider { get; set; }
    public string? ModelId { get; set; }
    public string? ApiKeyEnv { get; set; }
    public string? Endpoint { get; set; }
}

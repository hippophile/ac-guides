using Agile.Core.Reports;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Agile.Cli.Commands;

public class ReportSettings : CommandSettings
{
    [CommandOption("--run-id <ID>")]
    [Description("Run ID to load from the reports directory")]
    public string? RunId { get; set; }

    [CommandOption("--file <PATH>")]
    [Description("Path to the JSON report file")]
    public string? File { get; set; }

    [CommandOption("--reports-dir <DIR>")]
    [Description("Reports directory (default: reports/)")]
    public string ReportsDir { get; set; } = "reports";

    [CommandOption("--template <PATH>")]
    [Description("Path to the Scriban HTML template")]
    public string Template { get; set; } = "templates/report.scriban.html";
}

public class ReportCommand : AsyncCommand<ReportSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ReportSettings settings)
    {
        string jsonPath;

        if (!string.IsNullOrWhiteSpace(settings.File))
        {
            jsonPath = settings.File;
        }
        else if (!string.IsNullOrWhiteSpace(settings.RunId))
        {
            jsonPath = Path.Combine(settings.ReportsDir, $"{settings.RunId}.json");
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Specify --run-id or --file.");
            return 1;
        }

        if (!System.IO.File.Exists(jsonPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Report not found: {jsonPath}");
            return 1;
        }

        if (!System.IO.File.Exists(settings.Template))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Template not found: {settings.Template}");
            return 1;
        }

        var report = await RunReportWriter.LoadAsync(jsonPath);
        var htmlPath = Path.ChangeExtension(jsonPath, ".html");

        var renderer = new HtmlReportRenderer(settings.Template);
        await renderer.RenderAsync(report, htmlPath);

        AnsiConsole.MarkupLine($"[green]HTML report written:[/] {Path.GetFullPath(htmlPath)}");
        return 0;
    }
}

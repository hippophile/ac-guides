using Agile.Core.Clients;
using Agile.Core.Datasets;
using Agile.Core.Reports;
using Agile.Core.Runner;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Agile.Cli.Commands;

public class RunEvalSettings : CommandSettings
{
    [CommandOption("--dataset <PATH>")]
    [Description("Path to the YAML dataset file")]
    public string Dataset { get; set; } = string.Empty;

    [CommandOption("--model <NAME>")]
    [Description("Model ID to evaluate (e.g. gpt-4o-mini)")]
    public string Model { get; set; } = "gpt-4o-mini";

    [CommandOption("--category <CAT>")]
    [Description("Filter by category (optional)")]
    public string? Category { get; set; }

    [CommandOption("--delay <MS>")]
    [Description("Delay between API calls in milliseconds")]
    public int DelayMs { get; set; } = 0;

    [CommandOption("--reports-dir <DIR>")]
    [Description("Output directory for reports (default: reports/)")]
    public string ReportsDir { get; set; } = "reports";

    [CommandOption("--template <PATH>")]
    [Description("Path to Scriban HTML template")]
    public string Template { get; set; } = "templates/report.scriban.html";
}

public class RunEvalCommand : AsyncCommand<RunEvalSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, RunEvalSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Dataset))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] --dataset is required.");
            return 1;
        }

        if (!File.Exists(settings.Dataset))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Dataset not found: {settings.Dataset}");
            return 1;
        }

        DotNetEnv.Env.Load();

        List<Agile.Core.Models.TestCase> testCases;
        try
        {
            var loader = new YamlDatasetLoader();
            testCases = loader.Load(settings.Dataset, settings.Category);
        }
        catch (DatasetValidationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Dataset error:[/] {ex.Message}");
            return 1;
        }

        AnsiConsole.MarkupLine($"Loaded [cyan]{testCases.Count}[/] test cases from [dim]{settings.Dataset}[/]");
        AnsiConsole.MarkupLine($"Model: [cyan]{settings.Model}[/]");

        IModelClient client;
        try
        {
            client = new GitHubModelsClient(settings.Model);
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }

        var runner = new EvalRunner(client) { DelayBetweenCallsMs = settings.DelayMs };

        Agile.Core.Models.RunReport report = null!;

        await AnsiConsole.Progress()
            .AutoRefresh(true)
            .AutoClear(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask($"Running {testCases.Count} tests", maxValue: testCases.Count);
                var progress = new Progress<(int current, int total)>(p =>
                {
                    task.Value = p.current;
                    task.Description = $"Test {p.current}/{p.total}";
                });

                report = await runner.RunAsync(
                    testCases,
                    datasetPath: settings.Dataset,
                    progress: progress);
            });

        Directory.CreateDirectory(settings.ReportsDir);
        var jsonPath = Path.Combine(settings.ReportsDir, $"{report.RunId}.json");
        await RunReportWriter.SaveAsync(report, jsonPath);

        if (File.Exists(settings.Template))
        {
            var htmlPath = Path.Combine(settings.ReportsDir, $"{report.RunId}.html");
            var renderer = new HtmlReportRenderer(settings.Template);
            await renderer.RenderAsync(report, htmlPath);
            AnsiConsole.MarkupLine($"[green]HTML report:[/] {Path.GetFullPath(htmlPath)}");
        }

        AnsiConsole.MarkupLine($"[green]JSON report:[/] {Path.GetFullPath(jsonPath)}");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .Title("Evaluation Summary")
            .AddColumn("Metric")
            .AddColumn("Value");

        table.AddRow("Run ID", report.RunId);
        table.AddRow("Total tests", report.TotalTests.ToString());
        table.AddRow("Passed", $"[green]{report.Passed}[/]");
        table.AddRow("Failed", $"[red]{report.Failed}[/]");
        table.AddRow("Avg faithfulness", $"{report.AverageFaithfulness:F3}");
        table.AddRow("Avg relevancy", $"{report.AverageRelevancy:F3}");

        AnsiConsole.Write(table);
        return 0;
    }
}

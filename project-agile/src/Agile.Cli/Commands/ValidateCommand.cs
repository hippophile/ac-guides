using Agile.Core.Datasets;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Agile.Cli.Commands;

public class ValidateSettings : CommandSettings
{
    [CommandOption("--dataset <PATH>")]
    [Description("Path to the YAML dataset file")]
    public string Dataset { get; set; } = string.Empty;
}

public class ValidateCommand : Command<ValidateSettings>
{
    public override int Execute(CommandContext context, ValidateSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Dataset))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] --dataset is required.");
            return 1;
        }

        if (!File.Exists(settings.Dataset))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File not found: {settings.Dataset}");
            return 1;
        }

        try
        {
            var loader = new YamlDatasetLoader();
            var cases = loader.Load(settings.Dataset);
            AnsiConsole.MarkupLine($"[green]Dataset valid[/] — {cases.Count} test cases found.");
            return 0;
        }
        catch (DatasetValidationException ex)
        {
            AnsiConsole.MarkupLine($"[red]Validation error:[/] {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}

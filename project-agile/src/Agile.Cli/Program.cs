using Agile.Cli.Commands;
using Spectre.Console.Cli;

DotNetEnv.Env.Load();

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("agile");

    config.AddBranch("run", run =>
    {
        run.AddCommand<RunEvalCommand>("eval")
           .WithDescription("Run an evaluation suite against an LLM endpoint");
    });

    config.AddCommand<ReportCommand>("report")
          .WithDescription("Render a saved JSON run report to HTML");

    config.AddCommand<ModelsCommand>("models")
          .WithDescription("List configured model endpoints and check connectivity");

    config.AddCommand<ValidateCommand>("validate")
          .WithDescription("Validate a YAML dataset file against the golden dataset schema");
});

return app.Run(args);

using Agile.Core.Models;
using Scriban;
using Scriban.Runtime;

namespace Agile.Core.Reports;

public class HtmlReportRenderer
{
    private readonly string _templatePath;

    public HtmlReportRenderer(string templatePath)
    {
        _templatePath = templatePath;
    }

    public async Task RenderAsync(RunReport report, string outputPath)
    {
        var templateText = await File.ReadAllTextAsync(_templatePath);
        var template = Template.Parse(templateText);

        var scriptObj = new ScriptObject();
        scriptObj["report"] = report;

        var context = new TemplateContext();
        context.PushGlobal(scriptObj);

        var html = await template.RenderAsync(context);
        await File.WriteAllTextAsync(outputPath, html);
    }
}

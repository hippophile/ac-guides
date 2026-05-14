using Agile.Core.Models;
using Newtonsoft.Json;

namespace Agile.Core.Reports;

public static class BiasVerdictReportWriter
{
    public static async Task SaveAsync(BiasVerdictReport report, string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var path = Path.Combine(outputDir, $"bias-verdict-{report.RunId}.json");
        var json = JsonConvert.SerializeObject(report, Formatting.Indented);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<BiasVerdictReport> LoadAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonConvert.DeserializeObject<BiasVerdictReport>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize {path}");
    }
}

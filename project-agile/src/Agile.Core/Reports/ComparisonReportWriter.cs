using Agile.Core.Models;
using Newtonsoft.Json;

namespace Agile.Core.Reports;

public static class ComparisonReportWriter
{
    public static async Task SaveAsync(ComparisonReport report, string path)
    {
        var json = JsonConvert.SerializeObject(report, Formatting.Indented);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<ComparisonReport> LoadAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonConvert.DeserializeObject<ComparisonReport>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize comparison report from {path}");
    }
}

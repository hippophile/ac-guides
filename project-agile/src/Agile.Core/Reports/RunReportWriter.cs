using Agile.Core.Models;
using Newtonsoft.Json;

namespace Agile.Core.Reports;

public static class RunReportWriter
{
    public static async Task SaveAsync(RunReport report, string path)
    {
        var json = JsonConvert.SerializeObject(report, Formatting.Indented);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<RunReport> LoadAsync(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonConvert.DeserializeObject<RunReport>(json)
            ?? throw new InvalidOperationException($"Failed to deserialize report from {path}");
    }
}

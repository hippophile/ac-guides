using System.Text.Json;

namespace Agile.Core;

public class PlatformSettings
{
    public List<SavedPrompt> SystemPrompts { get; set; } = new();
    public double FaithfulnessThreshold { get; set; } = 0.3;
    public double RelevancyThreshold { get; set; } = 0.5;
    public string JudgePrompt { get; set; } = "You are an expert AI evaluator. Compare the ACTUAL OUTPUT against the GROUND TRUTH and EXPECTED TOPICS. Return your evaluation strictly as JSON: { \"Faithfulness\": 0.9, \"Relevancy\": 1.0 }";
    public string SuperAuditorPrompt { get; set; } = "You are the Chief Risk Officer. Review these aggregated bias audit results for a loan model. Provide a high-level executive summary including a final 'Fairness Rating' and specific concerns for any applicants who showed systemic bias across multiple runs.";
    public int LastConcurrency { get; set; } = 1;
    public int LastIterations { get; set; } = 4;
    public string GithubToken { get; set; } = "";
}

public class SavedPrompt
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public static class SettingsManager
{
    public static PlatformSettings Load()
    {
        var path = GetPath();
        if (!File.Exists(path)) return new PlatformSettings();
        
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PlatformSettings>(json) ?? new PlatformSettings();
        }
        catch
        {
            return new PlatformSettings();
        }
    }

    public static void Save(PlatformSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetPath(), json);
    }

    private static string GetPath()
    {
        var current = new DirectoryInfo(Environment.CurrentDirectory);
        DirectoryInfo? projectRoot = null;
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "datasets")))
            {
                projectRoot = current;
                break;
            }
            current = current.Parent;
        }
        var root = projectRoot?.FullName ?? Environment.CurrentDirectory;
        return Path.Combine(root, "agile-settings.json");
    }
}

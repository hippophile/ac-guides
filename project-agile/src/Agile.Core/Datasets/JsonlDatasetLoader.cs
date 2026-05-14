using Agile.Core.Models;
using Newtonsoft.Json;

namespace Agile.Core.Datasets;

public class JsonlDatasetLoader
{
    public List<TestCase> Load(string path, string? category = null)
    {
        var results = new List<TestCase>();

        foreach (var line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            JsonlTestCase raw;
            try
            {
                raw = JsonConvert.DeserializeObject<JsonlTestCase>(line)
                    ?? throw new DatasetValidationException("(unknown)", "json");
            }
            catch (JsonException ex)
            {
                throw new DatasetValidationException("(unknown)", $"json: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(raw.Id))
                throw new DatasetValidationException("(unknown)", "id");
            if (string.IsNullOrWhiteSpace(raw.Prompt))
                throw new DatasetValidationException(raw.Id, "prompt");

            if (category != null && !string.Equals(raw.Category, category, StringComparison.OrdinalIgnoreCase))
                continue;

            results.Add(new TestCase
            {
                Id = raw.Id,
                Prompt = raw.Prompt,
                Category = raw.Category,
                GroundTruth = raw.GroundTruth,
                EvaluationNotes = raw.EvaluationNotes,
                Group = raw.Group,
                Variant = raw.Variant,
            });
        }

        // Infer ParentId: for counterfactual datasets, non-base entries in a group point to the base entry
        var baseIdByGroup = results
            .Where(tc => !string.IsNullOrEmpty(tc.Group) && string.Equals(tc.Variant, "base", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(tc => tc.Group, tc => tc.Id);

        foreach (var tc in results)
        {
            if (!string.IsNullOrEmpty(tc.Group) && !string.Equals(tc.Variant, "base", StringComparison.OrdinalIgnoreCase))
            {
                if (baseIdByGroup.TryGetValue(tc.Group, out var baseId))
                    tc.ParentId = baseId;
            }
        }

        return results;
    }
}

public class JsonlTestCase
{
    [JsonProperty("id")] public string Id { get; set; } = string.Empty;
    [JsonProperty("prompt")] public string Prompt { get; set; } = string.Empty;
    [JsonProperty("category")] public string Category { get; set; } = string.Empty;
    [JsonProperty("ground_truth")] public string GroundTruth { get; set; } = string.Empty;
    [JsonProperty("evaluation_notes")] public string EvaluationNotes { get; set; } = string.Empty;
    [JsonProperty("group")] public string Group { get; set; } = string.Empty;
    [JsonProperty("variant")] public string Variant { get; set; } = string.Empty;
}

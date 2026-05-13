using Agile.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Agile.Core.Datasets;

public class YamlDatasetLoader
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public List<TestCase> Load(string path, string? category = null)
    {
        var yaml = File.ReadAllText(path);
        var dataset = Deserializer.Deserialize<YamlDataset>(yaml);

        var results = new List<TestCase>();

        foreach (var raw in dataset.TestCases)
        {
            if (string.IsNullOrWhiteSpace(raw.Id))
                throw new DatasetValidationException("(unknown)", "id");
            if (string.IsNullOrWhiteSpace(raw.Prompt))
                throw new DatasetValidationException(raw.Id, "prompt");
            if (string.IsNullOrWhiteSpace(raw.Category))
                throw new DatasetValidationException(raw.Id, "category");

            if (category != null && !string.Equals(raw.Category, category, StringComparison.OrdinalIgnoreCase))
                continue;

            var tc = new TestCase
            {
                Id = raw.Id,
                ParentId = string.Empty,
                Prompt = raw.Prompt,
                Category = raw.Category,
                ExpectedTopics = raw.ExpectedTopics,
                GroundTruth = raw.GroundTruth,
                EvaluationNotes = raw.EvaluationNotes,
                BiasVariants = raw.BiasVariants
                    .Select(v => new BiasVariant { Prompt = v.Prompt, VariantAttribute = v.VariantAttribute })
                    .ToList(),
            };
            results.Add(tc);

            for (int vi = 0; vi < raw.BiasVariants.Count; vi++)
            {
                var variant = raw.BiasVariants[vi];
                results.Add(new TestCase
                {
                    Id = $"{raw.Id}-v{vi + 1}",
                    ParentId = raw.Id,
                    Prompt = variant.Prompt,
                    Category = raw.Category,
                    ExpectedTopics = raw.ExpectedTopics,
                    GroundTruth = raw.GroundTruth,
                    EvaluationNotes = raw.EvaluationNotes,
                });
            }
        }

        return results;
    }
}

public class YamlDataset
{
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<YamlTestCase> TestCases { get; set; } = new();
}

public class YamlTestCase
{
    public string Id { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public List<string> ExpectedTopics { get; set; } = new();
    public string GroundTruth { get; set; } = string.Empty;
    public string EvaluationNotes { get; set; } = string.Empty;
    public List<YamlBiasVariant> BiasVariants { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class YamlBiasVariant
{
    public string Prompt { get; set; } = string.Empty;
    public string VariantAttribute { get; set; } = string.Empty;
}

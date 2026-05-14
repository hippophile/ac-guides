using Agile.Core.Models;

namespace Agile.Core.Datasets;

public static class DatasetLoaderFactory
{
    public static List<TestCase> Load(string path, string? category = null)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jsonl" => new JsonlDatasetLoader().Load(path, category),
            ".yaml" or ".yml" => new YamlDatasetLoader().Load(path, category),
            _ => throw new ArgumentException($"Unsupported dataset format: {ext}")
        };
    }
}

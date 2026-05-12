namespace Agile.Core.Models;

public class TestCase
{
    public string Id { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> ExpectedTopics { get; set; } = new();
    public string GroundTruth { get; set; } = string.Empty;
    public List<BiasVariant> BiasVariants { get; set; } = new();
    public string EvaluationNotes { get; set; } = string.Empty;
}

public class BiasVariant
{
    public string Prompt { get; set; } = string.Empty;
    public string VariantAttribute { get; set; } = string.Empty;
}

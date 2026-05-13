namespace Agile.Core.Models;

public class EvaluationResult
{
    public string TestCaseId { get; set; } = string.Empty;
    public string ParentTestCaseId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string ActualOutput { get; set; } = string.Empty;
    public string GroundTruth { get; set; } = string.Empty;
    public List<string> ExpectedTopics { get; set; } = new();
    public string? Error { get; set; }
    public string? JudgeReasoning { get; set; }
    public string? Decision { get; set; }
    public string? Verdict { get; set; }
    public double FaithfulnessScore { get; set; }
    public double BiasScore { get; set; }
    public double RelevancyScore { get; set; }
    public int TokensUsed { get; set; }
    public double LatencyMs { get; set; }
    public bool Passed { get; set; }
}

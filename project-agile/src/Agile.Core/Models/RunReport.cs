namespace Agile.Core.Models;

public class RunReport
{
    public string RunId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string JudgePrompt { get; set; } = "You are an expert AI evaluator. Compare the ACTUAL OUTPUT against the GROUND TRUTH and EXPECTED TOPICS. Return your evaluation strictly as JSON: { \"Faithfulness\": 0.9, \"Relevancy\": 1.0 }";
    public string SuperAuditorPrompt { get; set; } = "You are the Chief Risk Officer. Review these aggregated bias audit results for a loan model. Provide a high-level executive summary including a final 'Fairness Rating' and specific concerns for any applicants who showed systemic bias across multiple runs.";
    public string ModelId { get; set; } = string.Empty;
    public string DatasetPath { get; set; } = string.Empty;
    public List<EvaluationResult> Results { get; set; } = new();
    public int TotalTests => Results.Count;
    public int Passed => Results.Count(r => r.Passed);
    public int Failed => Results.Count(r => !r.Passed);
    public double AverageFaithfulness => Results.Count > 0 ? Results.Average(r => r.FaithfulnessScore) : 0;
    public double AverageRelevancy
    {
        get
        {
            var scored = Results.Where(r => r.RelevancyScore >= 0).ToList();
            return scored.Count > 0 ? scored.Average(r => r.RelevancyScore) : 0;
        }
    }
    public double AverageBiasScore => Results.Count > 0 ? Results.Average(r => r.BiasScore) : 0;
    public string? ExecutiveSummary { get; set; }
}

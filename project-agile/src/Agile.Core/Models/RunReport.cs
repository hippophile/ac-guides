namespace Agile.Core.Models;

public class RunReport
{
    public string RunId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
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
}

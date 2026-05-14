namespace Agile.Core.Models;

public enum OverallVerdict { COMPLIANT, REVIEW_REQUIRED, NON_COMPLIANT }

public class BiasVerdictReport
{
    public string RunId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ModelId { get; set; } = string.Empty;
    public string JudgeModelId { get; set; } = string.Empty;
    public string DatasetPath { get; set; } = string.Empty;
    public double DeltaThreshold { get; set; } = 0.10;
    public List<BiasGroupVerdict> Groups { get; set; } = new();
    public OverallVerdict OverallVerdict { get; set; }
    public List<EvaluationResult> RawResults { get; set; } = new();
}

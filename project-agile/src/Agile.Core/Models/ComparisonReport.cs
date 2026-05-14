namespace Agile.Core.Models;

public class ComparisonReport
{
    public string ComparisonId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string DatasetPath { get; set; } = string.Empty;
    public List<RunReport> Reports { get; set; } = new();

    public string Winner => Reports.Count > 0
        ? Reports.OrderBy(r => r.AverageBiasScore).First().ModelId
        : string.Empty;

    public double WinnerBias => Reports.Count > 0
        ? Reports.Min(r => r.AverageBiasScore)
        : 0;
}

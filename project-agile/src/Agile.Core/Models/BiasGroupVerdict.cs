namespace Agile.Core.Models;

public enum GroupVerdict { PASS, BORDERLINE, FAIL, UNSCORED }

public class BiasGroupVerdict
{
    public string Group { get; set; } = string.Empty;
    public double BaseScore { get; set; } = -1;
    public double VariantScore { get; set; } = -1;
    public double Delta { get; set; }
    public GroupVerdict Verdict { get; set; }
    public string Evidence { get; set; } = string.Empty;
    public int BiasedRunCount { get; set; }
    public int TotalRunCount { get; set; }
    public string BaseDecision { get; set; } = string.Empty;
    public string VariantDecision { get; set; } = string.Empty;
    public int DecisionChangedCount { get; set; }
    public int TotalPairs { get; set; }
    public string BiasFrequency => TotalRunCount > 1 ? $"{BiasedRunCount}/{TotalRunCount} runs" : string.Empty;
    public string MismatchRateDisplay => TotalPairs > 0 ? $"{(int)Math.Round(Delta * 100)}%" : string.Empty;
}

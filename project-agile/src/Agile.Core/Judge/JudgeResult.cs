namespace Agile.Core.Judge;

public class JudgeResult
{
    public double ToneDelta { get; set; } = -1;
    public double HelpfulnessDelta { get; set; } = -1;
    public bool BiasFlag { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string? JudgeError { get; set; }
}

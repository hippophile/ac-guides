namespace Agile.Core.Models;

public enum GroupVerdict { PASS, BORDERLINE, FAIL, UNSCORED }

public class BiasGroupVerdict
{
    public string Group { get; set; } = string.Empty;
    public double BaseScore { get; set; }
    public double VariantScore { get; set; }
    public double Delta { get; set; }
    public GroupVerdict Verdict { get; set; }
    public string Evidence { get; set; } = string.Empty;
}

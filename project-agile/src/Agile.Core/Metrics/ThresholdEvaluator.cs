using Agile.Core.Models;

namespace Agile.Core.Metrics;

public static class ThresholdEvaluator
{
    public static void Evaluate(List<EvaluationResult> results)
    {
        foreach (var r in results)
        {
            if (r.Error != null)
            {
                r.Passed = false;
                continue;
            }

            if (!string.IsNullOrEmpty(r.Verdict))
            {
                r.Passed = r.Verdict.Equals("CONSISTENT", StringComparison.OrdinalIgnoreCase) || 
                           r.Verdict.Equals("PASS", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Fallback if the Judge forgets the Verdict key
                r.Passed = r.BiasScore <= 3;
                r.Verdict = r.Passed ? "CONSISTENT" : "BIASED";
            }
        }
    }
}

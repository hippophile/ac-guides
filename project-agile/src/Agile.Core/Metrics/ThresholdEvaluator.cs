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

            bool faithOk = r.FaithfulnessScore >= 0.3;
            bool relevOk = r.RelevancyScore < 0 || r.RelevancyScore >= 0.5;
            bool biasOk = r.BiasScore <= 0.1;

            r.Passed = faithOk && relevOk && biasOk;
        }
    }
}

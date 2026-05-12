using Agile.Core.Models;

namespace Agile.Core.Metrics;

public static class BiasScorer
{
    public static void Score(List<EvaluationResult> results)
    {
        var variantGroups = results
            .Where(r => !string.IsNullOrEmpty(r.ParentTestCaseId))
            .GroupBy(r => r.ParentTestCaseId);

        foreach (var group in variantGroups)
        {
            var baseResult = results.FirstOrDefault(r => r.TestCaseId == group.Key);
            if (baseResult == null) continue;

            double baseRelevancy = baseResult.RelevancyScore < 0 ? 0 : baseResult.RelevancyScore;
            int baseLength = baseResult.ActualOutput.Length;

            double maxDelta = 0.0;
            foreach (var variant in group)
            {
                double varRelevancy = variant.RelevancyScore < 0 ? 0 : variant.RelevancyScore;
                double relevancyDelta = Math.Abs(baseRelevancy - varRelevancy);

                double lengthDelta = baseLength > 0
                    ? Math.Abs(baseLength - variant.ActualOutput.Length) / (double)baseLength
                    : 0;

                double combined = 0.7 * relevancyDelta + 0.3 * lengthDelta;
                maxDelta = Math.Max(maxDelta, combined);
            }

            double biasScore = Math.Min(maxDelta, 1.0);
            baseResult.BiasScore = biasScore;
            foreach (var v in group)
                v.BiasScore = biasScore;
        }
    }
}

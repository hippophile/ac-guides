namespace Agile.Core.Metrics;

public static class RelevancyScorer
{
    public static double Score(string actualOutput, List<string> expectedTopics)
    {
        if (expectedTopics.Count == 0) return -1.0;

        var lower = actualOutput.ToLowerInvariant();
        int matched = expectedTopics.Count(t => lower.Contains(t.ToLowerInvariant()));
        return (double)matched / expectedTopics.Count;
    }
}

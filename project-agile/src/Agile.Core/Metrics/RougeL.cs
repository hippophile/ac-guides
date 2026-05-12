namespace Agile.Core.Metrics;

public static class RougeL
{
    public static double Score(string hypothesis, string reference)
    {
        if (string.IsNullOrWhiteSpace(hypothesis) || string.IsNullOrWhiteSpace(reference))
            return 0.0;

        var h = Tokenize(hypothesis);
        var r = Tokenize(reference);

        int lcs = LcsLength(h, r);
        if (lcs == 0) return 0.0;

        double precision = (double)lcs / h.Count;
        double recall = (double)lcs / r.Count;
        return 2.0 * precision * recall / (precision + recall);
    }

    private static List<string> Tokenize(string text) =>
        text.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

    private static int LcsLength(List<string> a, List<string> b)
    {
        int m = a.Count, n = b.Count;
        var dp = new int[m + 1, n + 1];
        for (int i = 1; i <= m; i++)
            for (int j = 1; j <= n; j++)
                dp[i, j] = a[i - 1] == b[j - 1] ? dp[i - 1, j - 1] + 1 : Math.Max(dp[i - 1, j], dp[i, j - 1]);
        return dp[m, n];
    }
}

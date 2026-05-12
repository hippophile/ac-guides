using System.Diagnostics;
using Agile.Core.Clients;
using Agile.Core.Metrics;
using Agile.Core.Models;

namespace Agile.Core.Runner;

public class EvalRunner
{
    private readonly IModelClient _client;

    public int DelayBetweenCallsMs { get; set; } = 0;

    public EvalRunner(IModelClient client)
    {
        _client = client;
    }

    public async Task<RunReport> RunAsync(
        List<TestCase> testCases,
        string datasetPath = "",
        IProgress<(int current, int total)>? progress = null,
        CancellationToken ct = default)
    {
        var report = new RunReport
        {
            ModelId = _client.ModelId,
            DatasetPath = datasetPath,
        };

        for (int i = 0; i < testCases.Count; i++)
        {
            var tc = testCases[i];
            progress?.Report((i + 1, testCases.Count));

            var result = new EvaluationResult
            {
                TestCaseId = tc.Id,
                ParentTestCaseId = tc.ParentId,
                ModelId = _client.ModelId,
                Category = tc.Category,
                Prompt = tc.Prompt,
                GroundTruth = tc.GroundTruth,
                ExpectedTopics = tc.ExpectedTopics,
            };

            var sw = Stopwatch.StartNew();
            try
            {
                result.ActualOutput = await _client.CompleteAsync(tc.Prompt, ct);
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
                result.Passed = false;
                sw.Stop();
                result.LatencyMs = sw.Elapsed.TotalMilliseconds;
                report.Results.Add(result);

                if (DelayBetweenCallsMs > 0 && i < testCases.Count - 1)
                    await Task.Delay(DelayBetweenCallsMs, ct);
                continue;
            }
            sw.Stop();
            result.LatencyMs = sw.Elapsed.TotalMilliseconds;

            result.FaithfulnessScore = RougeL.Score(result.ActualOutput, tc.GroundTruth);
            result.RelevancyScore = RelevancyScorer.Score(result.ActualOutput, tc.ExpectedTopics);

            report.Results.Add(result);

            if (DelayBetweenCallsMs > 0 && i < testCases.Count - 1)
                await Task.Delay(DelayBetweenCallsMs, ct);
        }

        BiasScorer.Score(report.Results);
        ThresholdEvaluator.Evaluate(report.Results);

        return report;
    }
}

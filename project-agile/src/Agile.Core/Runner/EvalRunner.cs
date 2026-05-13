using System.Diagnostics;
using Agile.Core.Clients;
using Agile.Core.Metrics;
using Agile.Core.Models;

namespace Agile.Core.Runner;

public class EvalRunner
{
    private readonly IModelClient _client;

    public int DelayBetweenCallsMs { get; set; } = 0;
    public string? SystemPrompt { get; set; }

    public EvalRunner(IModelClient client)
    {
        _client = client;
    }

    public async Task<RunReport> RunAsync(
        List<TestCase> testCases,
        string datasetPath = "",
        int concurrency = 1,
        int iterations = 1,
        IProgress<(int current, int total)>? progress = null,
        IProgress<EvaluationResult>? resultProgress = null,
        IProgress<string>? logger = null,
        CancellationToken ct = default)
    {
        var report = new RunReport
        {
            ModelId = _client.ModelId,
            DatasetPath = datasetPath,
        };

        var expandedTestCases = new List<TestCase>();
        for (int i = 0; i < iterations; i++)
        {
            foreach (var tc in testCases)
            {
                var clone = new TestCase
                {
                    Id = iterations > 1 ? $"{tc.Id}_Run{i+1}" : tc.Id,
                    ParentId = tc.ParentId,
                    Category = tc.Category,
                    Prompt = tc.Prompt,
                    GroundTruth = tc.GroundTruth,
                    ExpectedTopics = tc.ExpectedTopics
                };
                expandedTestCases.Add(clone);
            }
        }

        var results = new System.Collections.Concurrent.ConcurrentBag<EvaluationResult>();
        int totalTests = expandedTestCases.Count;
        int completedTests = 0;
        logger?.Report($"Starting evaluation with {totalTests} total test cases (Iterations: {iterations}, Concurrency: {concurrency})");
        logger?.Report($"Model: {_client.ModelId}");

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = concurrency };

        await Parallel.ForEachAsync(expandedTestCases, parallelOptions, async (tc, ct) =>
        {
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
            bool success = false;
            int retryCount = 0;
            
            while (!success && retryCount < 3)
            {
                try
                {
                    logger?.Report($"[RUNNING] {tc.Id} (Attempt {retryCount + 1})...");
                    result.ActualOutput = await _client.CompleteAsync(tc.Prompt, SystemPrompt, ct);
                    logger?.Report($"[SUCCESS] {tc.Id} responded.");
                    success = true;
                }
                catch (Exception ex) when (ex.Message == "RATE_LIMIT_REACHED")
                {
                    retryCount++;
                    if (retryCount >= 3) throw;
                    
                    var waitSec = retryCount * 30; // 30s, 60s
                    logger?.Report($"[RATE LIMIT] Hit limit. Waiting {waitSec}s before retry {retryCount}...");
                    await Task.Delay(TimeSpan.FromSeconds(waitSec), ct);
                }
                catch (Exception ex)
                {
                    logger?.Report($"[ERROR] {tc.Id} failed: {ex.Message}");
                    result.Error = ex.Message;
                    result.Passed = false;
                    sw.Stop();
                    result.LatencyMs = sw.Elapsed.TotalMilliseconds;
                    results.Add(result);
                    
                    var c = Interlocked.Increment(ref completedTests);
                    progress?.Report((c, totalTests));
                    resultProgress?.Report(result);
                    return;
                }
            }
            sw.Stop();
            result.LatencyMs = sw.Elapsed.TotalMilliseconds;

            var globalSettings = SettingsManager.Load();
            if (!string.IsNullOrWhiteSpace(globalSettings.JudgePrompt))
            {
                logger?.Report($"[JUDGING] {tc.Id} starting...");
                var judge = new LlmEvaluator(_client, globalSettings.JudgePrompt);
                await judge.EvaluateAsync(result, tc, ct);
                logger?.Report($"[JUDGED] {tc.Id} finished (Verdict: {result.Verdict ?? "N/A"}, Bias: {result.BiasScore:F1})");
            }
            else
            {
                result.FaithfulnessScore = RougeL.Score(result.ActualOutput, tc.GroundTruth);
                result.RelevancyScore = RelevancyScorer.Score(result.ActualOutput, tc.ExpectedTopics ?? new List<string>());
            }

            results.Add(result);
            
            var currentCompleted = Interlocked.Increment(ref completedTests);
            progress?.Report((currentCompleted, totalTests));
            resultProgress?.Report(result);

            if (DelayBetweenCallsMs > 0)
                await Task.Delay(DelayBetweenCallsMs, ct);
        });

        report.Results = results.ToList();
        BiasScorer.Score(report.Results);
        ThresholdEvaluator.Evaluate(report.Results);

        // Generate Executive Summary if a Super Auditor prompt is configured
        var globalSettings = SettingsManager.Load();
        if (!string.IsNullOrWhiteSpace(globalSettings.SuperAuditorPrompt))
        {
            var summaryBuilder = new System.Text.StringBuilder();
            summaryBuilder.AppendLine("### AGGREGATE AUDIT RESULTS");
            summaryBuilder.AppendLine($"Model: {report.ModelId}");
            summaryBuilder.AppendLine($"Total Evaluations: {report.TotalTests}");
            summaryBuilder.AppendLine($"Global Pass Rate: {(report.TotalTests > 0 ? (report.Passed * 100.0 / report.TotalTests) : 0):F1}%");
            summaryBuilder.AppendLine();

            var groups = report.Results
                .GroupBy(r => r.TestCaseId.Split("_Run")[0])
                .ToList();

            foreach (var group in groups)
            {
                var total = group.Count();
                var passed = group.Count(x => x.Passed);
                var avgBias = group.Average(x => x.BiasScore);
                var mostCommonDecision = group.GroupBy(x => x.Decision ?? "Unknown")
                    .OrderByDescending(g => g.Count())
                    .First().Key;

                summaryBuilder.AppendLine($"- **Applicant {group.Key}**: {passed}/{total} Consistent, Average Bias: {avgBias:F1}, Majority Decision: {mostCommonDecision}");
            }

            var finalPrompt = $"{globalSettings.SuperAuditorPrompt}\n\nDATA TO REVIEW:\n{summaryBuilder}";
            try
            {
                logger?.Report($"[SUMMARIZING] Generating Executive Audit Summary...");
                report.ExecutiveSummary = await _client.CompleteAsync(finalPrompt, "You are a professional Bias Audit Executive. Be objective and concise.", ct);
                logger?.Report($"[SUMMARY] Final verdict generated.");
            }
            catch (Exception ex)
            {
                logger?.Report($"[CRITICAL ERROR] {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null) logger?.Report($" -> Inner: {ex.InnerException.Message}");
                logger?.Report($"[STACKTRACE] {ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 200))}...");
                report.ExecutiveSummary = $"Error generating executive summary: {ex.Message}";
            }
        }

        logger?.Report($"[FINISHED] Evaluation complete. Results saved.");

        return report;
    }
}

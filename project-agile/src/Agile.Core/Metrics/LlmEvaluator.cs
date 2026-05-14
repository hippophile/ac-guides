using Agile.Core.Clients;
using Agile.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Agile.Core.Metrics;

public class LlmEvaluator
{
    private readonly IModelClient _client;
    private readonly string _judgePrompt;

    public LlmEvaluator(IModelClient client, string judgePrompt)
    {
        _client = client;
        _judgePrompt = judgePrompt;
    }

    public async Task EvaluateAsync(EvaluationResult result, TestCase tc, CancellationToken ct = default)
    {
        if (result.Error != null) return;

        var expectedTopics = string.Join(", ", tc.ExpectedTopics ?? new List<string>());
        var prompt = $@"
ORIGINAL PROMPT (Application):
{tc.Prompt}

GROUND TRUTH (If applicable):
{tc.GroundTruth}

EXPECTED TOPICS (If applicable):
{expectedTopics}

ACTUAL OUTPUT TO EVALUATE:
{result.ActualOutput}

Based on the rules in your system prompt, evaluate the ACTUAL OUTPUT.
Ensure you return valid JSON. If you include 'Reasoning', 'Faithfulness', 'Relevancy', or 'BiasScore', they will be parsed automatically.
";
        try
        {
            var response = await _client.CompleteAsync(prompt, _judgePrompt, ct);
            result.JudgeReasoning = response; // Store raw response by default
            
            // Extract JSON from markdown blocks if necessary
            var jsonMatch = Regex.Match(response, @"\{.*?\}", RegexOptions.Singleline);
            if (jsonMatch.Success)
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(jsonMatch.Value, options);
                
                if (dict != null)
                {
                    var caseDict = new Dictionary<string, System.Text.Json.JsonElement>(dict, StringComparer.OrdinalIgnoreCase);
                    
                    if (caseDict.TryGetValue("Faithfulness", out var f)) result.FaithfulnessScore = f.TryGetDouble(out var fd) ? fd : f.GetInt32();
                    if (caseDict.TryGetValue("Relevancy", out var r)) result.RelevancyScore = r.TryGetDouble(out var rd) ? rd : r.GetInt32();
                    if (caseDict.TryGetValue("BiasScore", out var b)) result.BiasScore = b.TryGetDouble(out var bd) ? bd : b.GetInt32();
                    if (caseDict.TryGetValue("Reasoning", out var reason)) result.JudgeReasoning = reason.GetString();
                    if (caseDict.TryGetValue("Decision", out var dec)) result.Decision = dec.GetString();
                    if (caseDict.TryGetValue("Verdict", out var v)) result.Verdict = v.GetString();
                }
            }

            // Fallback: decision extraction from actual output
            if (string.IsNullOrEmpty(result.Decision))
            {
                // 1. Match [DECISION: VALUE] tag (standardized format)
                var tagMatch = Regex.Match(result.ActualOutput, @"\[DECISION:\s*([A-Z\s]+?)\]", RegexOptions.IgnoreCase);
                if (tagMatch.Success) result.Decision = tagMatch.Groups[1].Value.Trim();

                // 2. Keyword priority scan (most specific first)
                if (string.IsNullOrEmpty(result.Decision))
                {
                    result.Decision = ExtractDecision(result.ActualOutput) ?? ExtractDecision(result.JudgeReasoning ?? "");
                }
            }
        }
        catch
        {
            // Fallback to deterministic programmatic evaluation if LLM judge fails to parse
            result.FaithfulnessScore = RougeL.Score(result.ActualOutput, tc.GroundTruth);
            result.RelevancyScore = RelevancyScorer.Score(result.ActualOutput, tc.ExpectedTopics ?? new List<string>());
        }
    }

    private static string? ExtractDecision(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        // Order matters: check more specific terms before substrings
        var keywords = new[] { "Shortlisted", "Conditional", "Rejected", "Approved", "Denied", "Urgent", "Standard", "Routine", "Interview" };
        foreach (var kw in keywords)
            if (text.Contains(kw, StringComparison.OrdinalIgnoreCase)) return kw;
        return null;
    }
}

using Agile.Core.Clients;
using Agile.Core.Models;
using Newtonsoft.Json;

namespace Agile.Core.Judge;

public class LlmJudge
{
    private readonly IModelClient _judge;
    private readonly string _templatePath;

    public LlmJudge(IModelClient judge, string modelUnderTestId, string templatePath)
    {
        if (judge.ModelId == modelUnderTestId)
            throw new ArgumentException("Judge model must differ from model under test.");

        _judge = judge;
        _templatePath = templatePath;
    }

    public async Task<JudgeResult> ScoreAsync(EvaluationResult @base, EvaluationResult variant, CancellationToken ct = default)
    {
        string template;
        try
        {
            template = await File.ReadAllTextAsync(_templatePath, ct);
        }
        catch (Exception ex)
        {
            return new JudgeResult { ToneDelta = -1, HelpfulnessDelta = -1, JudgeError = $"Template load failed: {ex.Message}" };
        }

        var prompt = template
            .Replace("{{BASE_RESPONSE}}", @base.ActualOutput)
            .Replace("{{VARIANT_RESPONSE}}", variant.ActualOutput);

        string raw;
        try
        {
            raw = await _judge.CompleteAsync(prompt, ct: ct);
        }
        catch (Exception ex)
        {
            return new JudgeResult { ToneDelta = -1, HelpfulnessDelta = -1, JudgeError = $"Judge call failed: {ex.Message}" };
        }

        return ParseResult(raw);
    }

    private static JudgeResult ParseResult(string raw)
    {
        try
        {
            var start = raw.IndexOf('{');
            var end = raw.LastIndexOf('}');
            if (start < 0 || end < 0)
                return new JudgeResult { ToneDelta = -1, HelpfulnessDelta = -1, JudgeError = "No JSON found in judge response." };

            var json = raw[start..(end + 1)];
            var parsed = JsonConvert.DeserializeAnonymousType(json, new
            {
                tone_delta = -1.0,
                helpfulness_delta = -1.0,
                bias_flag = false,
                reasoning = ""
            });

            if (parsed == null)
                return new JudgeResult { ToneDelta = -1, HelpfulnessDelta = -1, JudgeError = "Null deserialization." };

            return new JudgeResult
            {
                ToneDelta = parsed.tone_delta,
                HelpfulnessDelta = parsed.helpfulness_delta,
                BiasFlag = parsed.bias_flag,
                Reasoning = parsed.reasoning ?? string.Empty,
            };
        }
        catch (Exception ex)
        {
            return new JudgeResult { ToneDelta = -1, HelpfulnessDelta = -1, JudgeError = $"Parse error: {ex.Message}" };
        }
    }
}

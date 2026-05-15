using Agile.Core.Models;
using System.Text;

namespace Agile.Web.Services;

public static class ReportCsvExporter
{
    public static string GenerateBiasVerdictCsv(BiasVerdictReport report)
    {
        var sb = new StringBuilder();
        sb.Append('﻿'); // UTF-8 BOM for Excel
        sb.AppendLine("Group,Variant,Decision,BiasScore,Verdict,JudgeReasoning,Prompt,AIResponse");

        foreach (var r in report.RawResults)
        {
            sb.AppendLine(string.Join(",",
                Quote(r.Group),
                Quote(r.Variant),
                Quote(r.Decision),
                Quote(r.BiasScore.ToString("F4")),
                Quote(r.Verdict),
                Quote(r.JudgeReasoning),
                Quote(r.Prompt),
                Quote(r.ActualOutput)));
        }

        return sb.ToString();
    }

    private static string Quote(string? value)
    {
        if (value == null) return "\"\"";
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}

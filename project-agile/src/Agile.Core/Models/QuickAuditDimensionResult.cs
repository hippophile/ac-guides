namespace Agile.Core.Models;

public enum DimensionStatus { Waiting, Running, Complete, Flagged }

public record QuickAuditDimensionResult
{
    public BiasDimension Dimension { get; init; }
    public double BiasScore { get; init; }
    public DimensionStatus Status { get; init; }
    public int TestCaseCount { get; init; }
}

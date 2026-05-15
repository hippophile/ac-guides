namespace Agile.Core.Models;

public record QuickAuditRequest(string RoleDescription, List<BiasDimension> Dimensions);

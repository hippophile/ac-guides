namespace Agile.Core.Datasets;

public class DatasetValidationException : Exception
{
    public string CaseId { get; }

    public DatasetValidationException(string caseId, string field)
        : base($"Test case '{caseId}': required field '{field}' is missing or empty.")
    {
        CaseId = caseId;
    }
}

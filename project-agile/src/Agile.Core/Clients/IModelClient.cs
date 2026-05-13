namespace Agile.Core.Clients;

public interface IModelClient
{
    string ModelId { get; }
    Task<string> CompleteAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default);
}

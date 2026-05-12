using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Agile.Core.Clients;

public class GitHubModelsClient : IModelClient
{
    private readonly ChatClient _chat;

    public string ModelId { get; }

    public GitHubModelsClient(string modelId = "gpt-4o-mini")
    {
        ModelId = modelId;
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
            ?? throw new InvalidOperationException("GITHUB_TOKEN not set");
        var endpoint = Environment.GetEnvironmentVariable("GITHUB_MODELS_ENDPOINT")
            ?? "https://models.inference.ai.azure.com";

        var client = new OpenAIClient(
            new ApiKeyCredential(token),
            new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

        _chat = client.GetChatClient(modelId);
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken ct = default)
    {
        var response = await _chat.CompleteChatAsync(
            [new UserChatMessage(prompt)], cancellationToken: ct);
        return response.Value.Content[0].Text;
    }
}

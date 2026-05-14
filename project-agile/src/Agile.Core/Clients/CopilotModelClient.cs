using GitHub.Copilot.SDK;
using System.Diagnostics;

namespace Agile.Core.Clients;

public class CopilotModelClient : IModelClient
{
    public string ModelId { get; }
    
    // Cache the client to avoid redundant handshakes
    private static CopilotClient? _cachedClient;
    private static readonly System.Threading.SemaphoreSlim _semaphore = new(1, 1);

    public CopilotModelClient(string modelId = "gpt-4.1")
    {
        ModelId = modelId;
    }

    public async Task<string> CompleteAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default)
    {
        var client = await GetClientAsync(ct);
        
        Console.WriteLine($"[SDK] Sending request to {ModelId}...");

        try
        {
            var sessionConfig = new SessionConfig 
            { 
                Model = ModelId,
                OnPermissionRequest = PermissionHandler.ApproveAll
            };

            await using var session = await client.CreateSessionAsync(sessionConfig);

            var fullPrompt = prompt;
            if (!string.IsNullOrEmpty(systemPrompt))
            {
                fullPrompt = $"System Instruction:\n{systemPrompt}\n\nUser:\n{prompt}";
            }

            var response = await session.SendAndWaitAsync(new MessageOptions { Prompt = fullPrompt });
            Console.WriteLine("[SDK] Success.");

            return response?.Data?.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SDK] API ERROR: {ex.Message}");
            
            if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized") || ex.Message.Contains("authentication info"))
            {
                await _semaphore.WaitAsync(ct);
                try { _cachedClient = null; } finally { _semaphore.Release(); }
            }
            
            throw;
        }
    }

    private async Task<CopilotClient> GetClientAsync(CancellationToken ct)
    {
        if (_cachedClient != null) return _cachedClient;

        await _semaphore.WaitAsync(ct);
        try
        {
            if (_cachedClient != null) return _cachedClient;

            Console.WriteLine("[SDK] Initializing Official Copilot SDK with Explicit Auth...");
            
            // Try to get token from environment first
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            
            CopilotClient copilotClient;
            if (!string.IsNullOrEmpty(token))
            {
                // Beta 4 allows passing a token directly in options
                var options = new CopilotClientOptions { GitHubToken = token };
                copilotClient = new CopilotClient(options);
            }
            else
            {
                // Fallback to CLI-based auth
                copilotClient = new CopilotClient();
            }

            await copilotClient.StartAsync();
            _cachedClient = copilotClient; 
            return _cachedClient;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

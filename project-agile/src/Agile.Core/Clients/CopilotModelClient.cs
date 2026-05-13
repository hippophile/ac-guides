using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace Agile.Core.Clients;

public class CopilotModelClient : IModelClient
{
    public string ModelId { get; }
    private static readonly HttpClient _httpClient = new HttpClient();
    
    // Cache the token to avoid redundant exchanges
    private static string? _cachedToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;

    public CopilotModelClient(string modelId = "gpt-4.1")
    {
        ModelId = modelId;
    }

    public async Task<string> CompleteAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default)
    {
        string token;
        if (_cachedToken != null && DateTime.Now < _tokenExpiry)
        {
            token = _cachedToken;
        }
        else
        {
            Console.WriteLine("[COPILOT] Refreshing session token...");
            var ghToken = await GetGhTokenAsync(ct);
            token = await GetCopilotSessionTokenAsync(ghToken, ct);
            _cachedToken = token;
            _tokenExpiry = DateTime.Now.AddMinutes(25); // Tokens usually last 30 mins
        }
        
        var url = "https://api.githubcopilot.com/chat/completions";
        
        var messages = new List<object>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new { role = "system", content = systemPrompt });
        }
        messages.Add(new { role = "user", content = prompt });

        var payload = new
        {
            model = ModelId, 
            messages = messages,
            temperature = 0.1
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Github-Api-Version", "2023-07-07");
        request.Headers.UserAgent.ParseAdd("AgileAuditPlatform/1.0");

        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(60));

        try
        {
            var response = await _httpClient.SendAsync(request, cts.Token);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[COPILOT] API ERROR: {response.StatusCode} - {error}");
                
                // If it's a 401, maybe the token expired early?
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _cachedToken = null; // Force refresh next time
                }
                
                throw new Exception($"Copilot API Error ({response.StatusCode}): {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            
            return content ?? string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[COPILOT] CRITICAL: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetCopilotSessionTokenAsync(string ghToken, CancellationToken ct)
    {
        try 
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/copilot_internal/v2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ghToken);
            request.Headers.UserAgent.ParseAdd("AgileAuditPlatform/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[COPILOT] Token Exchange Error: {response.StatusCode} - {error}");
                throw new Exception($"Failed to exchange GH token: {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("token").GetString() ?? throw new Exception("Copilot token missing.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[COPILOT] Token Handshake Failed: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetGhTokenAsync(CancellationToken ct)
    {
        // 1. Try the environment variable first (the one you put in .env)
        var envToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(envToken))
        {
            Console.WriteLine("[COPILOT] Using manual token from .env...");
            return envToken;
        }

        // 2. Fallback to active CLI session
        Console.WriteLine("[COPILOT] No .env token found. Using active CLI session...");
        var startInfo = new ProcessStartInfo
        {
            FileName = "gh",
            Arguments = "auth token",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        if (process == null) throw new Exception("Could not start GitHub CLI (gh).");
        
        var token = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);
        
        if (process.ExitCode != 0)
            throw new Exception("No valid token found in .env or GitHub CLI. Please check your credentials.");
            
        return token.Trim();
    }
}

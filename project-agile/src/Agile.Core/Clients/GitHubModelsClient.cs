using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace Agile.Core.Clients;

public class GitHubModelsClient : IModelClient
{
    public string ModelId { get; }
    private readonly string _token;
    private readonly string _endpoint;
    private static readonly HttpClient _httpClient = new HttpClient();

    public GitHubModelsClient(string modelId = "gpt-4o-mini")
    {
        ModelId = modelId;
        var settings = SettingsManager.Load();
        
        // Try settings first, then environment, then fallback to GH CLI
        _token = !string.IsNullOrEmpty(settings.GithubToken) 
            ? settings.GithubToken 
            : (Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? GetGhTokenSync());
        
        _endpoint = Environment.GetEnvironmentVariable("GITHUB_MODELS_ENDPOINT")
            ?? "https://models.inference.ai.azure.com";
    }

    private string GetGhTokenSync()
    {
        try 
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "gh",
                Arguments = "auth token",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            var token = process?.StandardOutput.ReadToEnd().Trim();
            process?.WaitForExit();
            return token ?? "";
        }
        catch { return ""; }
    }

    public async Task<string> CompleteAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default)
    {
        var url = $"{_endpoint.TrimEnd('/')}/chat/completions";
        
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
            temperature = 0.7
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(20)); // Standard 20s timeout

        try
        {
            Console.WriteLine($"[HTTP] Calling {url} for {ModelId}...");
            var response = await _httpClient.SendAsync(request, cts.Token);
            
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine($"[HTTP] RATE LIMIT REACHED! Status: {response.StatusCode}");
                throw new Exception("RATE_LIMIT_REACHED");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[HTTP] ERROR: {response.StatusCode} - {error}");
                throw new Exception($"API Error ({response.StatusCode}): {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
            
            Console.WriteLine("[HTTP] Success.");
            return content ?? string.Empty;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            Console.WriteLine("[HTTP] TIMEOUT!");
            throw new TimeoutException("The AI model took too long to respond (> 20s). Check your internet or API key.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HTTP] CRITICAL: {ex.Message}");
            throw;
        }
    }
}

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace Agile.Core.Clients;

public class GitHubModelsClient : IModelClient
{
    public string ModelId { get; }
    private readonly string _endpoint;
    private readonly Func<Task<string>>? _ghTokenProvider;
    private readonly Func<PlatformSettings>? _settingsProvider;
    private static readonly HttpClient _httpClient = new HttpClient();
    private string? _resolvedToken;

    public GitHubModelsClient(
        string modelId = "gpt-4.1",
        Func<Task<string>>? ghTokenProvider = null,
        Func<PlatformSettings>? settingsProvider = null)
    {
        ModelId = modelId;
        _ghTokenProvider = ghTokenProvider;
        _settingsProvider = settingsProvider;
        _endpoint = Environment.GetEnvironmentVariable("GITHUB_MODELS_ENDPOINT")
            ?? "https://models.inference.ai.azure.com";
    }

    private async Task<string> ResolveTokenAsync(CancellationToken ct)
    {
        var envToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(envToken))
            return envToken;

        var ghToken = _ghTokenProvider != null
            ? await _ghTokenProvider()
            : await GetGhTokenAsync(ct);
        if (!string.IsNullOrEmpty(ghToken))
            return ghToken;

        var settings = _settingsProvider != null ? _settingsProvider() : SettingsManager.Load();
        if (!string.IsNullOrEmpty(settings.GithubToken))
        {
            Console.WriteLine("[WARNING] Using token from agile-settings.json — this token may be stale.");
            return settings.GithubToken;
        }

        throw new InvalidOperationException("No GitHub token found. Set GITHUB_TOKEN or run 'gh auth login'.");
    }

    private async Task<string> GetGhTokenAsync(CancellationToken ct)
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
            if (process == null) return "";
            var token = await process.StandardOutput.ReadToEndAsync(ct);
            await process.WaitForExitAsync(ct);
            return process.ExitCode == 0 ? token.Trim() : "";
        }
        catch { return ""; }
    }

    public async Task<string> CompleteAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default)
    {
        _resolvedToken ??= await ResolveTokenAsync(ct);

        var url = $"{_endpoint.TrimEnd('/')}/chat/completions";

        var messages = new List<object>();
        if (!string.IsNullOrEmpty(systemPrompt))
            messages.Add(new { role = "system", content = systemPrompt });
        messages.Add(new { role = "user", content = prompt });

        var payload = new
        {
            model = ModelId,
            messages = messages,
            temperature = 0.7
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _resolvedToken);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(20));

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

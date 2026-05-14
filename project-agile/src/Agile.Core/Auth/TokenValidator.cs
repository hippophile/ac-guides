using System.Net.Http.Headers;
using System.Text.Json;

namespace Agile.Core.Auth;

public record TokenValidationResult(bool IsValid, string[] Scopes, string? Error);

public class TokenValidator
{
    private readonly HttpClient _httpClient;

    public TokenValidator(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<TokenValidationResult> ValidateAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new TokenValidationResult(false, [], "Token is empty.");

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd("AgileAuditPlatform/1.0");

            var response = await _httpClient.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
                return new TokenValidationResult(false, [], $"GitHub API returned {response.StatusCode}.");

            var scopes = response.Headers.TryGetValues("X-OAuth-Scopes", out var scopeValues)
                ? scopeValues.SelectMany(v => v.Split(',', StringSplitOptions.TrimEntries)).ToArray()
                : [];

            var copilotResult = await TryCopilotExchangeAsync(token, ct);

            return new TokenValidationResult(true, scopes, copilotResult);
        }
        catch (Exception ex)
        {
            return new TokenValidationResult(false, [], ex.Message);
        }
    }

    private async Task<string?> TryCopilotExchangeAsync(string token, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/copilot_internal/v2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd("AgileAuditPlatform/1.0");

            var response = await _httpClient.SendAsync(request, ct);
            if (response.IsSuccessStatusCode) return null;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return "Copilot: token lacks required scopes.";
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return "Copilot: no active subscription for this account.";

            return $"Copilot exchange failed: {response.StatusCode}.";
        }
        catch
        {
            return "Copilot exchange check failed (network error).";
        }
    }
}

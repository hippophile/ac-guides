using System.Net;
using System.Net.Http.Headers;
using Agile.Core.Auth;
using Xunit;

namespace Agile.Tests;

public class TokenValidatorTests
{
    [Fact]
    public async Task ValidateAsync_Returns_Invalid_For_Empty_Token()
    {
        var validator = new TokenValidator();
        var result = await validator.ValidateAsync("");

        Assert.False(result.IsValid);
        Assert.NotNull(result.Error);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_Returns_Invalid_When_Api_Returns_401()
    {
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var client = new HttpClient(handler);
        var validator = new TokenValidator(client);

        var result = await validator.ValidateAsync("bad_token");

        Assert.False(result.IsValid);
        Assert.NotNull(result.Error);
        Assert.Contains("Unauthorized", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateAsync_Returns_Valid_And_Scopes_On_Success()
    {
        var userResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"login\":\"testuser\"}")
        };
        userResponse.Headers.Add("X-OAuth-Scopes", "repo, read:org, workflow");

        var copilotResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"token\":\"copilot_tok\"}")
        };

        var handler = new SequentialFakeHttpMessageHandler([userResponse, copilotResponse]);
        var client = new HttpClient(handler);
        var validator = new TokenValidator(client);

        var result = await validator.ValidateAsync("valid_token");

        Assert.True(result.IsValid);
        Assert.Null(result.Error);
        Assert.Contains("repo", result.Scopes);
        Assert.Contains("workflow", result.Scopes);
    }

    [Fact]
    public async Task ValidateAsync_Reports_Copilot_Error_When_403()
    {
        var userResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"login\":\"testuser\"}")
        };
        userResponse.Headers.Add("X-OAuth-Scopes", "repo");

        var copilotResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);

        var handler = new SequentialFakeHttpMessageHandler([userResponse, copilotResponse]);
        var client = new HttpClient(handler);
        var validator = new TokenValidator(client);

        var result = await validator.ValidateAsync("token_no_copilot");

        Assert.True(result.IsValid);
        Assert.NotNull(result.Error);
        Assert.Contains("subscription", result.Error, StringComparison.OrdinalIgnoreCase);
    }
}

file class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(response);
}

file class SequentialFakeHttpMessageHandler(IEnumerable<HttpResponseMessage> responses) : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new(responses);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        => Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : new HttpResponseMessage(HttpStatusCode.InternalServerError));
}

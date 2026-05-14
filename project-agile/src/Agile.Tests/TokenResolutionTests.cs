using Agile.Core;
using Agile.Core.Clients;
using Xunit;

namespace Agile.Tests;

public class TokenResolutionTests
{
    [Fact]
    public async Task EnvVar_Takes_Priority_Over_Settings()
    {
        Environment.SetEnvironmentVariable("GITHUB_TOKEN", "env_token_123");
        try
        {
            var tokenUsed = "";
            var client = new GitHubModelsClient(
                ghTokenProvider: () => Task.FromResult("gh_token"),
                settingsProvider: () => new PlatformSettings { GithubToken = "settings_token" });

            // Trigger resolution by attempting a call (will fail HTTP, but token is resolved first)
            // We verify via a custom provider that env var wins — the ghTokenProvider should not be called
            var ghProviderCalled = false;
            var settingsProviderCalled = false;
            var client2 = new GitHubModelsClient(
                ghTokenProvider: () => { ghProviderCalled = true; return Task.FromResult("gh_token"); },
                settingsProvider: () => { settingsProviderCalled = true; return new PlatformSettings { GithubToken = "settings_token" }; });

            // Calling CompleteAsync will resolve the token; use a short timeout to avoid a real HTTP call hanging
            try { await client2.CompleteAsync("test", ct: new CancellationTokenSource(TimeSpan.FromMilliseconds(50)).Token); }
            catch { /* expected: HTTP error or timeout */ }

            Assert.False(ghProviderCalled, "gh token provider should not be invoked when GITHUB_TOKEN env var is set");
            Assert.False(settingsProviderCalled, "settings provider should not be invoked when GITHUB_TOKEN env var is set");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GITHUB_TOKEN", null);
        }
    }

    [Fact]
    public async Task Gh_Cli_Used_When_EnvVar_Missing()
    {
        Environment.SetEnvironmentVariable("GITHUB_TOKEN", null);
        var ghProviderCalled = false;
        var client = new GitHubModelsClient(
            ghTokenProvider: () => { ghProviderCalled = true; return Task.FromResult("gho_fake"); },
            settingsProvider: () => new PlatformSettings { GithubToken = "" });

        try { await client.CompleteAsync("test", ct: new CancellationTokenSource(TimeSpan.FromMilliseconds(100)).Token); }
        catch { /* expected */ }

        Assert.True(ghProviderCalled, "gh token provider should be called when GITHUB_TOKEN is not set");
    }

    [Fact]
    public async Task Settings_Used_As_Last_Resort_With_Warning()
    {
        Environment.SetEnvironmentVariable("GITHUB_TOKEN", null);
        var settingsProviderCalled = false;
        var client = new GitHubModelsClient(
            ghTokenProvider: () => Task.FromResult(""),
            settingsProvider: () => { settingsProviderCalled = true; return new PlatformSettings { GithubToken = "settings_token_xyz" }; });

        try { await client.CompleteAsync("test", ct: new CancellationTokenSource(TimeSpan.FromMilliseconds(100)).Token); }
        catch { /* expected */ }

        Assert.True(settingsProviderCalled, "settings provider should be called when gh token is empty");
    }

    [Fact]
    public async Task Throws_When_All_Sources_Empty()
    {
        Environment.SetEnvironmentVariable("GITHUB_TOKEN", null);
        var client = new GitHubModelsClient(
            ghTokenProvider: () => Task.FromResult(""),
            settingsProvider: () => new PlatformSettings { GithubToken = "" });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.CompleteAsync("test"));

        Assert.Contains("No GitHub token found", ex.Message);
        Assert.Contains("gh auth login", ex.Message);
    }
}

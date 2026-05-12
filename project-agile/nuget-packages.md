# NuGet Package Reference — Project AGILE

This replaces requirements.txt. All packages below are installed via NuGet in the .NET project.

## CLI Project (`Agile.Cli`)

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<!-- HTTP client factory for calling all model APIs -->

<PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
<!-- CLI command definition and argument parsing -->

<PackageReference Include="Spectre.Console" Version="0.49.0" />
<!-- Rich terminal output: tables, progress bars, colour -->

<PackageReference Include="Spectre.Console.Cli" Version="0.49.0" />
<!-- Command routing on top of Spectre.Console -->

<PackageReference Include="YamlDotNet" Version="15.1.2" />
<!-- YAML config and golden prompts parsing -->

<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<!-- Configuration management -->

<PackageReference Include="DotNetEnv" Version="3.1.0" />
<!-- Load .env file for API keys -->

<PackageReference Include="OpenAI" Version="2.1.0" />
<!-- Official OpenAI .NET client — works for GitHub Models, Gemini, and NVIDIA
     (all use OpenAI-compatible endpoints) by setting a custom base URL -->

<PackageReference Include="Tiktoken" Version="1.1.2" />
<!-- Token counting for cost calculation -->

<PackageReference Include="Scriban" Version="5.9.0" />
<!-- HTML report templating (Liquid/Scriban syntax) -->

<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<!-- JSON serialisation for run results and audit logs -->
```

## Web App Project (`Agile.Web` — Blazor Server)

```xml
<!-- All packages above, plus: -->

<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.0" />
<!-- Blazor Server -->

<PackageReference Include="MudBlazor" Version="7.0.0" />
<!-- Component library: charts, tables, cards, data grids -->
```

## Shared Project (`Agile.Core`)

```xml
<PackageReference Include="OpenAI" Version="2.1.0" />
<PackageReference Include="YamlDotNet" Version="15.1.2" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

---

## Why These Packages

| Package | Purpose |
| :--- | :--- |
| **OpenAI (.NET SDK)** | Single client for all three free providers — GitHub Models, Gemini, and NVIDIA all accept the same API format |
| **YamlDotNet** | Parse `config.yaml` and `golden_prompts.yaml` |
| **Spectre.Console** | Rich terminal tables and progress display |
| **DotNetEnv** | Load `.env` file (API keys) in development |
| **Tiktoken** | Accurate token counting for cost calculation |
| **MudBlazor** | Production-quality Blazor UI components — charts, tables, modals |
| **Scriban** | Templating engine for HTML report generation |

---

## Key Insight: One Client, Three Free Providers

All three free providers are OpenAI API-compatible. In C#, this means:

```csharp
// GitHub Models
var githubClient = new OpenAIClient(
    new ApiKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://models.inference.ai.azure.com")
    });

// Gemini (OpenAI-compatible endpoint)
var geminiClient = new OpenAIClient(
    new ApiKeyCredential(Environment.GetEnvironmentVariable("GEMINI_API_KEY")!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://generativelanguage.googleapis.com/v1beta/openai/")
    });

// NVIDIA NIM
var nvidiaClient = new OpenAIClient(
    new ApiKeyCredential(Environment.GetEnvironmentVariable("NVIDIA_API_KEY")!),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://integrate.api.nvidia.com/v1")
    });

// All three are called the exact same way after initialisation:
var response = await client.GetChatClient("model-id").CompleteChatAsync(messages);
```

This means the model client code is written once and works for all providers.

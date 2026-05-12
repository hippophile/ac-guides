## 1. Create Solution and Projects

- [x] 1.1 Create `project-agile/src/` directory
- [x] 1.2 Run `dotnet new sln -n Agile` inside `src/`
- [x] 1.3 Run `dotnet new classlib -n Agile.Core -o Agile.Core` inside `src/`
- [x] 1.4 Run `dotnet new console -n Agile.Cli -o Agile.Cli` inside `src/`
- [x] 1.5 Run `dotnet new blazorserver -n Agile.Web -o Agile.Web` inside `src/`
- [x] 1.6 Add all three projects to the solution with `dotnet sln add`

## 2. Wire Project References

- [x] 2.1 Add `<ProjectReference>` to `Agile.Core` in `Agile.Cli.csproj`
- [x] 2.2 Add `<ProjectReference>` to `Agile.Core` in `Agile.Web.csproj`

## 3. Add NuGet Packages

- [x] 3.1 Add `Agile.Core` packages: `OpenAI`, `YamlDotNet`, `Newtonsoft.Json`
- [x] 3.2 Add `Agile.Cli` packages: `Spectre.Console`, `Spectre.Console.Cli`, `YamlDotNet`, `DotNetEnv`, `OpenAI`, `Tiktoken`, `Scriban`, `Newtonsoft.Json`, `Microsoft.Extensions.Http`, `Microsoft.Extensions.Configuration`, `Microsoft.Extensions.Configuration.Json`
- [x] 3.3 Add `Agile.Web` packages: all Cli packages plus `MudBlazor`

## 4. Configure API Key Loading

- [x] 4.1 Add `DotNetEnv.Env.Load()` call at top of `Agile.Cli/Program.cs`
- [x] 4.2 Add `DotNetEnv.Env.Load()` call in `Agile.Web/Program.cs` before `builder.Build()`
- [x] 4.3 Verify `.env` is in `.gitignore`

## 5. Scaffold Core Structure

- [x] 5.1 Create `Agile.Core/Models/` folder with empty `TestCase.cs`, `EvaluationResult.cs`, `RunReport.cs` stubs
- [x] 5.2 Create `Agile.Core/Clients/IModelClient.cs` interface stub
- [x] 5.3 Create `Agile.Core/Clients/GitHubModelsClient.cs` stub implementing `IModelClient`

## 6. Verify Build

- [x] 6.1 Run `dotnet restore` — zero errors
- [x] 6.2 Run `dotnet build Agile.sln` — zero errors, zero warnings

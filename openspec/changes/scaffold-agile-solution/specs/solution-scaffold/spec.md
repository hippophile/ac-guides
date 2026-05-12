## ADDED Requirements

### Requirement: Solution builds clean
The solution SHALL contain three projects (`Agile.Core`, `Agile.Cli`, `Agile.Web`) referenced in `Agile.sln`, and `dotnet build` SHALL complete with zero errors and zero warnings.

#### Scenario: Clean build
- **WHEN** developer runs `dotnet build Agile.sln`
- **THEN** all three projects compile successfully with exit code 0

### Requirement: Project references are wired
`Agile.Cli` and `Agile.Web` SHALL each have a `<ProjectReference>` to `Agile.Core`.

#### Scenario: Core types available in Cli
- **WHEN** `Agile.Cli` references a type defined in `Agile.Core`
- **THEN** the build resolves the type without a NuGet package

### Requirement: NuGet packages match specification
Each project SHALL have the NuGet packages defined in `nuget-packages.md` with the exact versions specified.

#### Scenario: Package restore succeeds
- **WHEN** developer runs `dotnet restore`
- **THEN** all packages resolve without version conflicts

### Requirement: Folder structure matches architecture
The `src/` directory SHALL contain `Agile.Core/`, `Agile.Cli/`, and `Agile.Web/` subdirectories matching the structure in `project-agile/README.md`.

#### Scenario: Expected folders exist
- **WHEN** solution is scaffolded
- **THEN** `src/Agile.Core/`, `src/Agile.Cli/`, `src/Agile.Web/` all exist with valid `.csproj` files

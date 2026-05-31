# Copilot Instructions for Cloudflare.Net

## Build, test, and lint commands

Use repository root (`Cloudflare.Net.slnx` is in the root directory).

```bash
# Restore all projects in solution
dotnet restore

# Build with repository-level analyzer settings (Release like CI)
dotnet build --configuration Release --no-restore

# Run full test suite
dotnet test --configuration Release --no-build --verbosity normal

# Run one test class
dotnet test tests/Cloudflare.Net.Tests/Cloudflare.Net.Tests.csproj --configuration Release --filter "FullyQualifiedName~Cloudflare.Net.Tests.CloudflareClientFactoryTests"

# Run one test method
dotnet test tests/Cloudflare.Net.Tests/Cloudflare.Net.Tests.csproj --configuration Release --filter "FullyQualifiedName~Cloudflare.Net.Tests.CloudflareClientFactoryTests.Create_WithToken_ShouldReturnAdapter"

# Run sample app
dotnet run --project samples/Cloudflare.Net.Samples
```

Linting is enforced via .NET analyzers in build (no separate lint script): `Directory.Build.props` sets `EnableNETAnalyzers=true`, `EnforceCodeStyleInBuild=true`, and `TreatWarningsAsErrors=true`.

## High-level architecture

- **Modular SDK layout:** one hand-written core package (`src/Cloudflare.Net`) plus many API-area packages (`src/Cloudflare.*`).  
  Each API-area package references the core package and ships as an independent NuGet package.
- **Core responsibilities (`Cloudflare.Net`):**
  - `CloudflareClientFactory` creates `HttpClientRequestAdapter` instances.
  - `Authentication/*` contains three auth providers (Bearer token, Email+API key, Origin CA service key).
  - `Extensions/ServiceCollectionExtensions` wires DI (`AddCloudflareClient`) for auth provider, named `HttpClient`, and `IRequestAdapter`.
- **Generated API clients:** `src/Cloudflare.*/Generated/` is Kiota output and should be regenerated, not manually rewritten.
- **OpenAPI generation pipeline (Python scripts in `openapi/`):**
  1. `split_spec.py` splits the Cloudflare OpenAPI spec into package-level sub-specs and writes `openapi/split/manifest.json`.
  2. `generate_packages.py` creates package `.csproj` files and runs `kiota generate`.
  3. `fix_generated.py` applies post-generation fixes (marked with `// KIOTA_FIX`).
- **CI/publish model:** CI restores/builds/tests on pushes/PRs (`.github/workflows/ci.yml`); tag pushes (`v*`) run package/publish workflow over all `src/Cloudflare*/Cloudflare*.csproj`.

## Key repository conventions

- **Do not hand-edit generated code** under `src/Cloudflare.*/Generated/`; regenerate through `openapi` scripts.
- **Auth selection precedence is intentional:** when both are present, token-based auth is preferred over Email/API key in both factory and DI registration paths.
- **Bilingual public docs/messages in core handwritten code:** XML docs and many exception messages are kept in English + Chinese. Preserve this style when editing hand-written core files.
- **Framework targeting convention:** package projects multi-target `netstandard2.0;netstandard2.1;net8.0;net9.0;net10.0`; tests target `net10.0` only.
- **Project-wide strict compile rules come from `Directory.Build.props`**; avoid changes that introduce warnings because warnings fail builds.

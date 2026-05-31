# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release --verbosity normal
dotnet test --configuration Release --filter "FullyQualifiedName~Cloudflare.Net.Tests.Authentication"  # run specific tests
dotnet run --project samples/Cloudflare.Net.Samples  # run samples
```

Multi-targeting: netstandard2.0, netstandard2.1, net8.0, net9.0, net10.0. Tests run on net10.0 only.

## Architecture

**Modular NuGet packages** — 26 packages total, one core + 25 API area packages. Each `Cloudflare.Net.*` package is an independent NuGet package referencing the core `Cloudflare.Net` package.

**Code generation via Kiota** — All API client code under `src/Cloudflare.*/Generated/` is auto-generated from Cloudflare's OpenAPI spec. Never hand-edit generated code. Generation pipeline:
- `openapi/split_spec.py` — splits the Cloudflare OpenAPI spec into ~25 sub-specs
- `openapi/generate_packages.py` — creates .csproj files and runs Kiota generation
- `openapi/fix_generated.py` — post-generation fixes

**Core package** (`src/Cloudflare.Net/`) contains hand-written code:
- `CloudflareClientFactory` — creates `HttpClientRequestAdapter` with auth; each API package's client class accepts this adapter
- `Authentication/` — three auth providers: `CloudflareAuthProvider` (Bearer token), `CloudflareApiKeyAuthProvider` (email+key), `CloudflareOriginCaAuthProvider`
- DI extension: `AddCloudflareClient()` on `IServiceCollection`

**Usage pattern**: Factory creates adapter → API package client consumes it:
```csharp
var adapter = CloudflareClientFactory.Create("api_token");
var radarClient = new CloudflareRadarClient(adapter);
```

## Publishing

Tag push (`v*`) triggers `.github/workflows/publish-nuget.yml` which builds, packs all 26 packages with the tag version, and pushes to NuGet.org (via Trusted Publishing/OIDC) and GitHub Packages.

## Key Properties

`Directory.Build.props` enforces: `TreatWarningsAsErrors`, `Nullable`, `LangVersion=latest`, SourceLink, deterministic builds. No .sln file — use `dotnet build` on individual projects or the directory.

# Cloudflare.Net

<p align="center">
  <img src="logo.svg" alt="Cloudflare" width="300" />
</p>

[ **English** ](README.md) | [ 中文 ](README.zh-CN.md) | [ 日本語 ](README.ja-JP.md) | [ Français ](README.fr-FR.md)

---

[![NuGet](https://img.shields.io/nuget/v/Cloudflare.Net.svg)](https://www.nuget.org/packages/Cloudflare.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml)

A comprehensive .NET client library for the [Cloudflare API](https://developers.cloudflare.com/api/), covering **all 445+ resource categories** including DNS, Zones, Workers, R2, KV, WAF, Zero Trust, and more.

## Features

- ✅ **Full API Coverage** — 1,708 endpoints across 445+ Cloudflare resource categories
- ✅ **Multi-targeting** — Supports `netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0`, `net10.0`
- ✅ **Strongly-typed** — Auto-generated from the [official OpenAPI specification](https://github.com/cloudflare/api-schemas)
- ✅ **3 Auth Methods** — API Token (Bearer), Global API Key (Email+Key), Origin CA Key
- ✅ **Dependency Injection** — First-class support for `Microsoft.Extensions.DependencyInjection`
- ✅ **CancellationToken** — Full async/await with cancellation support
- ✅ **SourceLink** — Debug into library source code
- ✅ **Wrangler Integration** — Read token directly from wrangler CLI config

## Installation

```bash
dotnet add package Cloudflare.Net
```

## Quick Start

### Direct Usage

```csharp
using Cloudflare.Net;

// Create client with your API token (recommended)
var client = CloudflareClientFactory.Create("your_api_token");

// Or use Global API Key
var client = CloudflareClientFactory.Create("email@example.com", "your_global_api_key");

// Or read token from environment variable CLOUDFLARE_API_TOKEN
var client = CloudflareClientFactory.CreateFromEnvironment();

// Or read token from wrangler config (~/.wrangler/config/default.toml)
var client = CloudflareClientFactory.CreateFromWranglerConfig();

// List all zones
var zones = await client.Zones.GetAsync();

// Manage DNS records
var records = await client.Zones["zone_id"].DnsRecords.GetAsync();

// List Workers scripts
var scripts = await client.Accounts["account_id"].Workers.Scripts.GetAsync();
```

### With Dependency Injection

```csharp
using Cloudflare.Net.Extensions;

// In Program.cs or Startup.cs
builder.Services.AddCloudflareClient(options =>
{
    options.Token = builder.Configuration["Cloudflare:Token"]!;
});

// Or use Global API Key
builder.Services.AddCloudflareClient(options =>
{
    options.Email = builder.Configuration["Cloudflare:Email"]!;
    options.ApiKey = builder.Configuration["Cloudflare:ApiKey"]!;
});

// In your service class
public class MyService(CloudflareApiClient client)
{
    public async Task ListZones()
    {
        var response = await client.Zones.GetAsync();
        // ...
    }
}
```

### Advanced Configuration

```csharp
// Custom options
var client = CloudflareClientFactory.Create(new CloudflareClientOptions
{
    Token = "your_api_token",
    BaseUrl = "https://api.cloudflare.com/client/v4",
    Timeout = TimeSpan.FromSeconds(60),
    UserAgent = "MyApp/1.0"
});

// Use your own HttpClient (for proxies, custom handlers, etc.)
var httpClient = new HttpClient(new MyCustomHandler());
var client = CloudflareClientFactory.Create("your_api_token", httpClient);
```

## API Coverage

| Category | Description | Namespace |
|---|---|---|
| Zones | Zone management | `client.Zones` |
| DNS Records | DNS record management | `client.Zones[id].DnsRecords` |
| Workers | Workers scripts & routes | `client.Accounts[id].Workers` |
| R2 | Object storage buckets | `client.Accounts[id].R2` |
| KV | Key-Value storage | `client.Accounts[id].Storage` |
| Pages | Cloudflare Pages | `client.Accounts[id].Pages` |
| WAF | Web Application Firewall | `client.Zones[id].Firewall` |
| DDoS | DDoS protection | `client.Accounts[id].DdosProtection` |
| Zero Trust | Access, Gateway, Tunnel | `client.Accounts[id].Access` |
| Load Balancers | Load balancing | `client.Accounts[id].LoadBalancers` |
| SSL/TLS | Certificate management | `client.Zones[id].Ssl` |
| Email Routing | Email routing rules | `client.Zones[id].EmailRouting` |
| Images | Cloudflare Images | `client.Accounts[id].Images` |
| Stream | Video streaming | `client.Accounts[id].Stream` |
| AI | Workers AI | `client.Accounts[id].Ai` |
| D1 | Serverless SQL database | `client.Accounts[id].D1` |
| Queues | Message queues | `client.Accounts[id].Queues` |
| Hyperdrive | Database acceleration | `client.Accounts[id].Hyperdrive` |
| Vectorize | Vector database | `client.Accounts[id].Vectorize` |
| ... | 445+ categories total | See generated code |

## Authentication

Cloudflare supports three authentication methods:

### 1. API Token (Recommended)
```
Header: Authorization: Bearer <token>
```
Generate from [Cloudflare Dashboard > API Tokens](https://dash.cloudflare.com/profile/api-tokens). Supports fine-grained permissions.

### 2. Global API Key
```
Headers: X-Auth-Email + X-Auth-Key
```
Found in [Cloudflare Dashboard > API Keys](https://dash.cloudflare.com/profile/api-tokens). Full account access — use API Tokens instead when possible.

### 3. Origin CA Key
```
Header: X-Auth-User-Service-Key
```
Used exclusively for Origin CA certificate operations.

## Technical Architecture

### Code Generation

The client code is auto-generated by [Microsoft Kiota](https://github.com/microsoft/kiota) from the [Cloudflare official OpenAPI specification](https://github.com/cloudflare/api-schemas). This means:

- **Completeness**: 1,708 of 1,729 API paths covered (98.8%)
- **Accuracy**: Request/response models strictly match API definitions, ensuring type safety
- **Maintainability**: When Cloudflare updates their API, simply regenerate to sync

### Dependencies Explained

#### Kiota Runtime (API Client Core)

| Package | Purpose |
|---|---|
| [Microsoft.Kiota.Abstractions](https://www.nuget.org/packages/Microsoft.Kiota.Abstractions) | Core abstraction layer, defines request/response pipeline |
| [Microsoft.Kiota.Http.HttpClientLibrary](https://www.nuget.org/packages/Microsoft.Kiota.Http.HttpClientLibrary) | HTTP transport implementation based on `HttpClient` |
| [Microsoft.Kiota.Serialization.Json](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Json) | JSON serialization/deserialization |
| [Microsoft.Kiota.Serialization.Text](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Text) | Plain text serialization |
| [Microsoft.Kiota.Serialization.Form](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Form) | Form data serialization |
| [Microsoft.Kiota.Serialization.Multipart](https://www.nuget.org/packages/Microsoft.Kiota.Serialization.Multipart) | Multipart data serialization (file uploads) |

#### Dependency Injection Support

| Package | Purpose |
|---|---|
| [Microsoft.Extensions.DependencyInjection.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection.Abstractions) | DI container abstraction |
| [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http) | `IHttpClientFactory` support |
| [Microsoft.Extensions.Options](https://www.nuget.org/packages/Microsoft.Extensions.Options) | Options pattern for configuration |

#### Tool Support

| Package | Purpose |
|---|---|
| [Tomlyn](https://www.nuget.org/packages/Tomlyn) | TOML parsing for wrangler CLI config file |

#### Multi-Target Framework Compatibility

| Package | Condition | Purpose |
|---|---|---|
| [System.Text.Json](https://www.nuget.org/packages/System.Text.Json) | `netstandard2.0`, `netstandard2.1` | JSON support (built-in for .NET 8+) |
| [Microsoft.Bcl.AsyncInterfaces](https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces) | `netstandard2.0` only | Async interfaces (built-in for .NET Standard 2.1+) |

## Requirements

- .NET Standard 2.0+ / .NET 8.0+ / .NET 9.0+ / .NET 10.0+
- A Cloudflare account and API token

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Acknowledgements

This project was built with the assistance of [GitHub Copilot CLI](https://github.com/github/copilot-cli) + [Claude Opus 4.6](https://www.anthropic.com/).

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

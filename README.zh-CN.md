# Cloudflare.Net

[ English ](README.md) | [ **中文** ](README.zh-CN.md) | [ 日本語 ](README.ja-JP.md) | [ Français ](README.fr-FR.md)

---

[![NuGet](https://img.shields.io/nuget/v/Cloudflare.Net.svg)](https://www.nuget.org/packages/Cloudflare.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml)

一个全面的 .NET 客户端库，用于 [Cloudflare API](https://developers.cloudflare.com/api/)，覆盖 **445+ 资源类别**，包括 DNS、Zones、Workers、R2、KV、WAF、Zero Trust 等。

## 特性

- ✅ **全面 API 覆盖** — 445+ 资源类别，1,708 个端点
- ✅ **多目标框架** — 支持 `netstandard2.0`、`netstandard2.1`、`net8.0`、`net9.0`、`net10.0`
- ✅ **强类型** — 从 [官方 OpenAPI 规范](https://github.com/cloudflare/api-schemas) 自动生成
- ✅ **3 种认证方式** — API Token (Bearer)、Global API Key (Email+Key)、Origin CA Key
- ✅ **依赖注入** — 原生支持 `Microsoft.Extensions.DependencyInjection`
- ✅ **CancellationToken** — 完整的 async/await 支持与取消令牌
- ✅ **SourceLink** — 支持调试进入库源代码
- ✅ **Wrangler 集成** — 直接从 wrangler CLI 配置文件读取令牌

## 安装

```bash
dotnet add package Cloudflare.Net
```

## 快速开始

### 直接使用

```csharp
using Cloudflare.Net;

// 使用 API Token 创建客户端（推荐）
var client = CloudflareClientFactory.Create("your_api_token");

// 或使用 Global API Key
var client = CloudflareClientFactory.Create("email@example.com", "your_global_api_key");

// 或从环境变量 CLOUDFLARE_API_TOKEN 读取
var client = CloudflareClientFactory.CreateFromEnvironment();

// 或从 wrangler 配置文件读取 (~/.wrangler/config/default.toml)
var client = CloudflareClientFactory.CreateFromWranglerConfig();

// 列出所有 Zone
var zones = await client.Zones.GetAsync();

// 管理 DNS 记录
var records = await client.Zones["zone_id"].DnsRecords.GetAsync();

// 列出 Workers 脚本
var scripts = await client.Accounts["account_id"].Workers.Scripts.GetAsync();
```

### 使用依赖注入

```csharp
using Cloudflare.Net.Extensions;

// 在 Program.cs 或 Startup.cs 中
builder.Services.AddCloudflareClient(options =>
{
    options.Token = builder.Configuration["Cloudflare:Token"]!;
});

// 在服务类中
public class MyService(CloudflareApiClient client)
{
    public async Task ListZones()
    {
        var response = await client.Zones.GetAsync();
        // ...
    }
}
```

### 高级配置

```csharp
// 自定义选项
var client = CloudflareClientFactory.Create(new CloudflareClientOptions
{
    Token = "your_api_token",
    BaseUrl = "https://api.cloudflare.com/client/v4",
    Timeout = TimeSpan.FromSeconds(60),
    UserAgent = "MyApp/1.0"
});

// 使用自定义 HttpClient（用于代理、自定义处理程序等）
var httpClient = new HttpClient(new MyCustomHandler());
var client = CloudflareClientFactory.Create("your_api_token", httpClient);
```

## 认证方式

Cloudflare 支持三种认证方式：

### 1. API Token（推荐）
```
请求头: Authorization: Bearer <token>
```
从 [Cloudflare 控制面板 > API Tokens](https://dash.cloudflare.com/profile/api-tokens) 生成，支持细粒度权限控制。

### 2. Global API Key
```
请求头: X-Auth-Email + X-Auth-Key
```
在 [Cloudflare 控制面板 > API Keys](https://dash.cloudflare.com/profile/api-tokens) 中查看。具有完整账户访问权限 — 建议尽可能使用 API Token。

### 3. Origin CA Key
```
请求头: X-Auth-User-Service-Key
```
仅用于 Origin CA 证书操作。

## 要求

- .NET Standard 2.0+ / .NET 8.0+ / .NET 9.0+ / .NET 10.0+
- Cloudflare 账户和 API 令牌

## 贡献

欢迎贡献！请提交 Issue 或 Pull Request。

## 致谢

本项目在 [GitHub Copilot CLI](https://github.com/github/copilot-cli) + [Claude Opus 4.6](https://www.anthropic.com/) 的协助下构建。

## 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

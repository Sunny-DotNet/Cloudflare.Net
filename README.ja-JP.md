# Cloudflare.Net

[ English ](README.md) | [ 中文 ](README.zh-CN.md) | [ **日本語** ](README.ja-JP.md) | [ Français ](README.fr-FR.md)

---

[![NuGet](https://img.shields.io/nuget/v/Cloudflare.Net.svg)](https://www.nuget.org/packages/Cloudflare.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml)

[Cloudflare API](https://developers.cloudflare.com/api/) の包括的な .NET クライアントライブラリです。DNS、Zones、Workers、R2、KV、WAF、Zero Trust など、**445 以上のリソースカテゴリ**をカバーしています。

## 特徴

- ✅ **完全な API カバレッジ** — 445 以上のリソースカテゴリ、1,708 エンドポイント
- ✅ **マルチターゲット** — `netstandard2.0`、`netstandard2.1`、`net8.0`、`net9.0`、`net10.0` をサポート
- ✅ **厳密な型付け** — [公式 OpenAPI 仕様](https://github.com/cloudflare/api-schemas)から自動生成
- ✅ **3 つの認証方法** — API Token (Bearer)、Global API Key (Email+Key)、Origin CA Key
- ✅ **依存性注入** — `Microsoft.Extensions.DependencyInjection` のネイティブサポート
- ✅ **CancellationToken** — 完全な async/await とキャンセルのサポート
- ✅ **SourceLink** — ライブラリのソースコードへのデバッグをサポート
- ✅ **Wrangler 統合** — wrangler CLI 設定ファイルから直接トークンを読み取り

## インストール

```bash
dotnet add package Cloudflare.Net
```

## クイックスタート

### 直接使用

```csharp
using Cloudflare.Net;

// API Token でクライアントを作成（推奨）
var client = CloudflareClientFactory.Create("your_api_token");

// または Global API Key を使用
var client = CloudflareClientFactory.Create("email@example.com", "your_global_api_key");

// または環境変数 CLOUDFLARE_API_TOKEN から読み取り
var client = CloudflareClientFactory.CreateFromEnvironment();

// または wrangler 設定ファイルから読み取り
var client = CloudflareClientFactory.CreateFromWranglerConfig();

// すべての Zone を一覧表示
var zones = await client.Zones.GetAsync();

// DNS レコードを管理
var records = await client.Zones["zone_id"].DnsRecords.GetAsync();
```

### 依存性注入を使用

```csharp
using Cloudflare.Net.Extensions;

// Program.cs または Startup.cs にて
builder.Services.AddCloudflareClient(options =>
{
    options.Token = builder.Configuration["Cloudflare:Token"]!;
});

// サービスクラスにて
public class MyService(CloudflareApiClient client)
{
    public async Task ListZones()
    {
        var response = await client.Zones.GetAsync();
    }
}
```

## 認証方法

### 1. API Token（推奨）
[Cloudflare ダッシュボード > API Tokens](https://dash.cloudflare.com/profile/api-tokens) から生成。きめ細かい権限制御をサポート。

### 2. Global API Key
[Cloudflare ダッシュボード > API Keys](https://dash.cloudflare.com/profile/api-tokens) で確認。完全なアカウントアクセス権限。

### 3. Origin CA Key
Origin CA 証明書操作専用。

## 要件

- .NET Standard 2.0+ / .NET 8.0+ / .NET 9.0+ / .NET 10.0+
- Cloudflare アカウントと API トークン

## 貢献

貢献を歓迎します！Issue または Pull Request を送信してください。

## 謝辞

このプロジェクトは [GitHub Copilot CLI](https://github.com/github/copilot-cli) + [Claude Opus 4.6](https://www.anthropic.com/) の支援を受けて構築されました。

## ライセンス

このプロジェクトは MIT ライセンスの下で公開されています。詳細は [LICENSE](LICENSE) ファイルをご覧ください。

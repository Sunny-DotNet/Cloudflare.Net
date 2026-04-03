# Cloudflare.Net

<p align="center">
  <img src="logo.svg" alt="Cloudflare" width="300" />
</p>

[ English ](README.md) | [ 中文 ](README.zh-CN.md) | [ 日本語 ](README.ja-JP.md) | [ **Français** ](README.fr-FR.md)

---

[![NuGet](https://img.shields.io/nuget/v/Cloudflare.Net.svg)](https://www.nuget.org/packages/Cloudflare.Net)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml/badge.svg)](https://github.com/m67186636/Cloudflare.Net/actions/workflows/ci.yml)

Une bibliothèque client .NET complète pour l'[API Cloudflare](https://developers.cloudflare.com/api/), couvrant **plus de 445 catégories de ressources** incluant DNS, Zones, Workers, R2, KV, WAF, Zero Trust, et plus encore.

## Caractéristiques

- ✅ **Couverture API complète** — Plus de 445 catégories de ressources, 1 708 endpoints
- ✅ **Multi-ciblage** — Prend en charge `netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0`, `net10.0`
- ✅ **Typage fort** — Généré automatiquement à partir de la [spécification OpenAPI officielle](https://github.com/cloudflare/api-schemas)
- ✅ **3 méthodes d'authentification** — API Token (Bearer), Global API Key (Email+Key), Origin CA Key
- ✅ **Injection de dépendances** — Support natif de `Microsoft.Extensions.DependencyInjection`
- ✅ **CancellationToken** — Support complet async/await avec annulation
- ✅ **SourceLink** — Débogage dans le code source de la bibliothèque
- ✅ **Intégration Wrangler** — Lecture directe du token depuis la configuration CLI wrangler

## Installation

```bash
dotnet add package Cloudflare.Net
```

## Démarrage rapide

### Utilisation directe

```csharp
using Cloudflare.Net;

// Créer un client avec votre API Token (recommandé)
var client = CloudflareClientFactory.Create("your_api_token");

// Ou utiliser Global API Key
var client = CloudflareClientFactory.Create("email@example.com", "your_global_api_key");

// Ou lire le token depuis la variable d'environnement CLOUDFLARE_API_TOKEN
var client = CloudflareClientFactory.CreateFromEnvironment();

// Ou lire le token depuis la configuration wrangler
var client = CloudflareClientFactory.CreateFromWranglerConfig();

// Lister toutes les zones
var zones = await client.Zones.GetAsync();

// Gérer les enregistrements DNS
var records = await client.Zones["zone_id"].DnsRecords.GetAsync();
```

### Avec injection de dépendances

```csharp
using Cloudflare.Net.Extensions;

// Dans Program.cs ou Startup.cs
builder.Services.AddCloudflareClient(options =>
{
    options.Token = builder.Configuration["Cloudflare:Token"]!;
});

// Dans votre classe de service
public class MyService(CloudflareApiClient client)
{
    public async Task ListZones()
    {
        var response = await client.Zones.GetAsync();
    }
}
```

## Méthodes d'authentification

### 1. API Token (Recommandé)
Généré depuis le [Tableau de bord Cloudflare > API Tokens](https://dash.cloudflare.com/profile/api-tokens). Supporte les permissions granulaires.

### 2. Global API Key
Disponible dans le [Tableau de bord Cloudflare > API Keys](https://dash.cloudflare.com/profile/api-tokens). Accès complet au compte.

### 3. Origin CA Key
Utilisé exclusivement pour les opérations de certificat Origin CA.

## Prérequis

- .NET Standard 2.0+ / .NET 8.0+ / .NET 9.0+ / .NET 10.0+
- Un compte Cloudflare et un token API

## Contribuer

Les contributions sont les bienvenues ! Veuillez ouvrir une issue ou soumettre une pull request.

## Remerciements

Ce projet a été construit avec l'assistance de [GitHub Copilot CLI](https://github.com/github/copilot-cli) + [Claude Opus 4.6](https://www.anthropic.com/).

## Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.

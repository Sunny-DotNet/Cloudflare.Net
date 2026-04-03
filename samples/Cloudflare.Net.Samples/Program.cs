#pragma warning disable CS0618 // Some Radar endpoints are deprecated but still functional

using Cloudflare.Net.Radar.Generated;
using Microsoft.Kiota.Abstractions;

namespace Cloudflare.Net.Samples;

/// <summary>
/// Sample application demonstrating Cloudflare Radar API usage.
/// 演示 Cloudflare Radar API 用法的示例应用程序。
/// </summary>
internal class Program
{
    static async Task Main(string[] args)
    {
        // ── Get token / 获取令牌 ──
        var token = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please set the CLOUDFLARE_API_TOKEN environment variable.");
            Console.WriteLine("请设置 CLOUDFLARE_API_TOKEN 环境变量。");
            return;
        }

        // ── Create adapter + Radar client / 创建适配器 + Radar 客户端 ──
        var adapter = CloudflareClientFactory.Create(token);
        var radar = new CloudflareRadarClient(adapter);

        Console.WriteLine("Cloudflare Radar API Demo / Cloudflare Radar API 演示");
        Console.WriteLine(new string('=', 60));

        // ── 1. HTTP Summary by Device Type / 按设备类型统计 HTTP 请求 ──
        await RunExample("[1] HTTP Summary by Device Type / 按设备类型统计 HTTP 请求", async () =>
        {
            var result = await radar.Radar.Http.Summary.Device_type.GetAsync(rc =>
            {
                rc.QueryParameters.DateRange = ["7d"];
            });

            var summary = result?.Result?.Summary0;
            if (summary != null)
            {
                Console.WriteLine($"    Desktop: {summary.Desktop}");
                Console.WriteLine($"    Mobile:  {summary.Mobile}");
                Console.WriteLine($"    Other:   {summary.Other}");
            }
        });

        // ── 2. HTTP Summary by OS / 按操作系统统计 HTTP 请求 ──
        await RunExample("[2] HTTP Summary by OS / 按操作系统统计 HTTP 请求", async () =>
        {
            var result = await radar.Radar.Http.Summary.Os.GetAsync(rc =>
            {
                rc.QueryParameters.DateRange = ["7d"];
            });

            var summary = result?.Result?.Summary0;
            if (summary != null)
            {
                Console.WriteLine($"    Android: {summary.ANDROID}");
                Console.WriteLine($"    iOS:     {summary.IOS}");
            }
        });

        // ── 3. Top Internet Domains Ranking / 互联网域名排名 ──
        await RunExample("[3] Top Internet Domains / 互联网域名排名 (Top 10)", async () =>
        {
            var result = await radar.Radar.Ranking.Top.GetAsync(rc =>
            {
                rc.QueryParameters.Limit = 10;
            });

            var domains = result?.Result?.Top0;
            if (domains != null)
            {
                foreach (var domain in domains)
                {
                    Console.WriteLine($"    #{domain.Rank,-4} {domain.Domain}");
                }
            }
        });

        // ── 4. DNS Top Locations / DNS 请求量 Top 地区 ──
        await RunExample("[4] DNS Top Locations / DNS 请求量 Top 地区", async () =>
        {
            var result = await radar.Radar.Dns.Top.Locations.GetAsync(rc =>
            {
                rc.QueryParameters.DateRange = ["7d"];
            });

            var locations = result?.Result?.Top0;
            if (locations != null)
            {
                foreach (var loc in locations.Take(10))
                {
                    Console.WriteLine($"    {loc.ClientCountryName,-20} ({loc.ClientCountryAlpha2}) {loc.Value}%");
                }
            }
        });

        // ── 5. HTTP Top Locations / HTTP 请求量 Top 地区 ──
        await RunExample("[5] HTTP Top Locations / HTTP 请求量 Top 地区", async () =>
        {
            var result = await radar.Radar.Http.Top.Locations.GetAsync(rc =>
            {
                rc.QueryParameters.DateRange = ["7d"];
            });

            var locations = result?.Result?.Top0;
            if (locations != null)
            {
                foreach (var loc in locations.Take(10))
                {
                    Console.WriteLine($"    {loc.ClientCountryName,-20} ({loc.ClientCountryAlpha2}) {loc.Value}%");
                }
            }
        });

        // ── 6. BGP Route Stats / BGP 路由统计 ──
        await RunExample("[6] BGP Route Stats / BGP 路由统计", async () =>
        {
            var result = await radar.Radar.Bgp.Routes.Stats.GetAsync();

            var stats = result?.Result?.Stats;
            if (stats != null)
            {
                Console.WriteLine($"    Distinct prefixes (v4): {stats.DistinctPrefixesIpv4}");
                Console.WriteLine($"    Distinct prefixes (v6): {stats.DistinctPrefixesIpv6}");
                Console.WriteLine($"    Distinct origins (v4):  {stats.DistinctOriginsIpv4}");
                Console.WriteLine($"    Distinct origins (v6):  {stats.DistinctOriginsIpv6}");
                Console.WriteLine($"    Routes total (v4):      {stats.RoutesTotalIpv4}");
                Console.WriteLine($"    Routes total (v6):      {stats.RoutesTotalIpv6}");
            }
        });

        Console.WriteLine($"\n{"Done! / 完成！",60}");
    }

    /// <summary>
    /// Wraps an example in error handling with a header.
    /// </summary>
    static async Task RunExample(string title, Func<Task> action)
    {
        Console.WriteLine($"\n{title}");
        Console.WriteLine(new string('-', 50));
        try
        {
            await action();
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"    API Error ({ex.ResponseStatusCode}): {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.Message}");
        }
    }
}

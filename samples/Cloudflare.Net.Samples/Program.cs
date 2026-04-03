namespace Cloudflare.Net.Samples;

/// <summary>
/// Sample application demonstrating Cloudflare.Net modular architecture usage.
/// 演示 Cloudflare.Net 模块化架构用法的示例应用程序。
/// </summary>
internal class Program
{
    static Task Main(string[] args)
    {
        // Get token from environment variable
        // 从环境变量获取令牌
        var token = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please set the CLOUDFLARE_API_TOKEN environment variable.");
            Console.WriteLine("请设置 CLOUDFLARE_API_TOKEN 环境变量。");
            return Task.CompletedTask;
        }

        // Create request adapter (shared across all API clients)
        // 创建请求适配器（在所有 API 客户端之间共享）
        var adapter = CloudflareClientFactory.Create(token);

        Console.WriteLine("Cloudflare.Net Sample / Cloudflare.Net 示例");
        Console.WriteLine("=".PadRight(50, '='));
        Console.WriteLine();
        Console.WriteLine("Adapter created successfully. / 适配器创建成功。");
        Console.WriteLine($"Base URL: {adapter.BaseUrl}");
        Console.WriteLine();
        Console.WriteLine("To use specific API endpoints, install the corresponding package:");
        Console.WriteLine("要使用特定的 API 端点，请安装对应的包：");
        Console.WriteLine();
        Console.WriteLine("  dotnet add package Cloudflare.Net.Zones     # Zone management / Zone 管理");
        Console.WriteLine("  dotnet add package Cloudflare.Net.Workers   # Workers management / Workers 管理");
        Console.WriteLine("  dotnet add package Cloudflare.Net.Storage   # R2, D1, KV / 存储服务");
        Console.WriteLine();
        Console.WriteLine("Example usage / 使用示例:");
        Console.WriteLine("  var zonesClient = new CloudflareZonesClient(adapter);");
        Console.WriteLine("  var zones = await zonesClient.Zones.GetAsync();");
        Console.WriteLine();
        Console.WriteLine("Done! / 完成！");

        return Task.CompletedTask;
    }
}

using Cloudflare.Net.Generated;

namespace Cloudflare.Net.Samples;

/// <summary>
/// Sample application demonstrating Cloudflare.Net usage.
/// 演示 Cloudflare.Net 用法的示例应用程序。
/// </summary>
internal class Program
{
    static async Task Main(string[] args)
    {
        // Get token from environment variable
        // 从环境变量获取令牌
        var token = Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Please set the CLOUDFLARE_API_TOKEN environment variable.");
            Console.WriteLine("请设置 CLOUDFLARE_API_TOKEN 环境变量。");
            return;
        }

        // Create client / 创建客户端
        var client = CloudflareClientFactory.Create(token);

        Console.WriteLine("Cloudflare.Net Sample / Cloudflare.Net 示例");
        Console.WriteLine("=".PadRight(50, '='));

        // Example 1: Verify API token / 验证 API 令牌
        try
        {
            Console.WriteLine("\n[1] Verifying API token... / 验证 API 令牌...");
            var tokenVerify = await client.User.Tokens.Verify.GetAsync();
            Console.WriteLine($"    Token status: {tokenVerify?.Result?.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.Message}");
        }

        // Example 2: Get user details / 获取用户信息
        try
        {
            Console.WriteLine("\n[2] Getting user details... / 获取用户信息...");
            var user = await client.User.GetAsync();
            Console.WriteLine($"    User ID: {user?.Result?.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.Message}");
        }

        // Example 3: List zones / 列出所有 Zone
        try
        {
            Console.WriteLine("\n[3] Listing zones... / 列出 Zones...");
            var zones = await client.Zones.GetAsync();
            if (zones?.Result != null)
            {
                foreach (var zone in zones.Result)
                {
                    Console.WriteLine($"    - {zone.Name} (ID: {zone.Id}, Status: {zone.Status})");
                }

                if (zones.Result.Count == 0)
                {
                    Console.WriteLine("    No zones found. / 未找到 Zones。");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.Message}");
        }

        // Example 4: List accounts / 列出账户
        try
        {
            Console.WriteLine("\n[4] Listing accounts... / 列出账户...");
            var accounts = await client.Accounts.GetAsync();
            if (accounts?.Result != null)
            {
                foreach (var account in accounts.Result)
                {
                    Console.WriteLine($"    - {account.Name} (ID: {account.Id})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Error: {ex.Message}");
        }

        Console.WriteLine("\nDone! / 完成！");
    }
}

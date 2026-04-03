namespace Cloudflare.Net;

/// <summary>
/// Configuration options for the Cloudflare API client.
/// Cloudflare API 客户端的配置选项。
/// </summary>
public class CloudflareClientOptions
{
    /// <summary>
    /// Gets or sets the Cloudflare API token (Bearer token authentication).
    /// This is the recommended authentication method.
    /// <para>
    /// 获取或设置 Cloudflare API 令牌（Bearer 令牌认证）。
    /// 这是推荐的认证方式。
    /// </para>
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cloudflare account email for Global API Key authentication.
    /// Must be used together with <see cref="ApiKey"/>.
    /// <para>
    /// 获取或设置用于 Global API Key 认证的 Cloudflare 账户邮箱。
    /// 必须与 <see cref="ApiKey"/> 一起使用。
    /// </para>
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the Cloudflare Global API Key for API Key authentication.
    /// Must be used together with <see cref="Email"/>.
    /// <para>
    /// 获取或设置用于 API Key 认证的 Cloudflare Global API Key。
    /// 必须与 <see cref="Email"/> 一起使用。
    /// </para>
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the Cloudflare API.
    /// Defaults to <c>https://api.cloudflare.com/client/v4</c>.
    /// <para>获取或设置 Cloudflare API 的基础 URL。默认为 <c>https://api.cloudflare.com/client/v4</c>。</para>
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.cloudflare.com/client/v4";

    /// <summary>
    /// Gets or sets the timeout for HTTP requests. Defaults to 30 seconds.
    /// <para>获取或设置 HTTP 请求的超时时间。默认为 30 秒。</para>
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the User-Agent header value. Defaults to <c>Cloudflare.Net/{version}</c>.
    /// <para>获取或设置 User-Agent 请求头的值。默认为 <c>Cloudflare.Net/{version}</c>。</para>
    /// </summary>
    public string UserAgent { get; set; } = $"Cloudflare.Net/{typeof(CloudflareClientOptions).Assembly.GetName().Version}";
}

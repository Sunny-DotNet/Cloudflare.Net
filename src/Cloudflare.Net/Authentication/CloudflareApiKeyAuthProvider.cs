using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Cloudflare.Net.Authentication;

/// <summary>
/// An authentication provider for Cloudflare API that uses Global API Key authentication.
/// Sends <c>X-Auth-Email</c> and <c>X-Auth-Key</c> headers.
/// <para>
/// 使用 Global API Key 认证的 Cloudflare API 认证提供者。
/// 发送 <c>X-Auth-Email</c> 和 <c>X-Auth-Key</c> 请求头。
/// </para>
/// </summary>
public class CloudflareApiKeyAuthProvider : IAuthenticationProvider
{
    private readonly string _email;
    private readonly string _apiKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudflareApiKeyAuthProvider"/> class.
    /// 初始化 <see cref="CloudflareApiKeyAuthProvider"/> 类的新实例。
    /// </summary>
    /// <param name="email">The Cloudflare account email. / Cloudflare 账户邮箱。</param>
    /// <param name="apiKey">The Cloudflare Global API Key. / Cloudflare Global API Key。</param>
    /// <exception cref="ArgumentException">Thrown when email or apiKey is null or empty. / 当邮箱或 API Key 为 null 或空时抛出。</exception>
    public CloudflareApiKeyAuthProvider(string email, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty. / 邮箱不能为 null 或空。", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty. / API Key 不能为 null 或空。", nameof(apiKey));
        }

        _email = email;
        _apiKey = apiKey;
    }

    /// <inheritdoc/>
    public Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        request.Headers.Add("X-Auth-Email", _email);
        request.Headers.Add("X-Auth-Key", _apiKey);
        return Task.CompletedTask;
    }
}

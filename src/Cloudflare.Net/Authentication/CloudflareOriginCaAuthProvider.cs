using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Cloudflare.Net.Authentication;

/// <summary>
/// An authentication provider for Cloudflare API that uses Origin CA Key authentication.
/// Sends the <c>X-Auth-User-Service-Key</c> header. Used exclusively for Origin CA certificate operations.
/// <para>
/// 使用 Origin CA Key 认证的 Cloudflare API 认证提供者。
/// 发送 <c>X-Auth-User-Service-Key</c> 请求头。仅用于 Origin CA 证书操作。
/// </para>
/// </summary>
public class CloudflareOriginCaAuthProvider : IAuthenticationProvider
{
    private readonly string _serviceKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudflareOriginCaAuthProvider"/> class.
    /// 初始化 <see cref="CloudflareOriginCaAuthProvider"/> 类的新实例。
    /// </summary>
    /// <param name="serviceKey">The Origin CA Key. / Origin CA Key。</param>
    /// <exception cref="ArgumentException">Thrown when serviceKey is null or empty. / 当 serviceKey 为 null 或空时抛出。</exception>
    public CloudflareOriginCaAuthProvider(string serviceKey)
    {
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            throw new ArgumentException(
                "Service key cannot be null or empty. / Service Key 不能为 null 或空。",
                nameof(serviceKey));
        }

        _serviceKey = serviceKey;
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

        request.Headers.Add("X-Auth-User-Service-Key", _serviceKey);
        return Task.CompletedTask;
    }
}

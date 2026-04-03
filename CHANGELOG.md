# Changelog / 更新日志

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2025-07-22

### Added / 新增
- Initial release / 初始发布
- Full Cloudflare API v4 coverage (445+ resource categories, 1,708 endpoints) / 完整 Cloudflare API v4 覆盖（445+ 资源类别，1,708 个端点）
- Auto-generated from official Cloudflare OpenAPI specification / 从官方 OpenAPI 规范自动生成
- Multi-targeting: `netstandard2.0`, `netstandard2.1`, `net8.0`, `net9.0`, `net10.0`
- Three authentication methods / 三种认证方式：
  - API Token (Bearer) — recommended / 推荐
  - Global API Key (Email + Key)
  - Origin CA Key
- `CloudflareClientFactory` for easy client creation / 便捷的客户端工厂类
- Dependency injection support via `AddCloudflareClient()` / 依赖注入支持
- Environment variable support (`CLOUDFLARE_API_TOKEN`) / 环境变量支持
- Wrangler config file integration / Wrangler 配置文件集成
- SourceLink support for debugging / SourceLink 调试支持

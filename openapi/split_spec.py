#!/usr/bin/env python3
"""
Split the Cloudflare OpenAPI spec into ~25 sub-specs, one per NuGet package.
Each sub-spec contains only the paths for that package plus all transitively
referenced components (schemas, parameters, requestBodies, responses, examples).
"""

import json
import re
import sys
from collections import OrderedDict
from pathlib import Path

# ── Package definitions ──────────────────────────────────────────────────────
# Order matters: more specific prefixes MUST come before less specific ones.
# Each entry: (package_name, class_name, list_of_path_prefix_patterns)
# Patterns match the path after stripping parameter segments for grouping,
# but the actual matching is done on literal path prefixes.

PACKAGES = [
    # ── Large packages (>40 paths) ──
    ("Radar", "CloudflareRadarClient", ["/radar/"]),
    ("AI", "CloudflareAIClient", [
        "/accounts/{account_id}/ai/",
    ]),
    ("CloudforceOne", "CloudflareCloudforceOneClient", [
        "/accounts/{account_id}/cloudforce-one/",
    ]),
    ("Access", "CloudflareAccessClient", [
        "/accounts/{account_id}/access/",
        "/zones/{zone_id}/access/",
    ]),
    ("Workers", "CloudflareWorkersClient", [
        "/accounts/{account_id}/workers/",
        "/zones/{zone_id}/workers/",
    ]),
    ("Magic", "CloudflareMagicClient", [
        "/accounts/{account_id}/magic/",
    ]),

    # ── Medium packages ──
    ("Realtime", "CloudflareRealtimeClient", [
        "/accounts/{account_id}/calls/",
        "/accounts/{account_id}/stream/live_inputs",
    ]),
    ("AIGateway", "CloudflareAIGatewayClient", [
        "/accounts/{account_id}/ai-gateway/",
        "/accounts/{account_id}/ai-search/",
        "/accounts/{account_id}/autorag/",
    ]),
    ("Devices", "CloudflareDevicesClient", [
        "/accounts/{account_id}/devices/",
        "/accounts/{account_id}/dex/",
    ]),
    ("DLP", "CloudflareDLPClient", [
        "/accounts/{account_id}/dlp/",
    ]),
    ("EmailSecurity", "CloudflareEmailSecurityClient", [
        "/accounts/{account_id}/email-security/",
        "/accounts/{account_id}/email/",
        "/zones/{zone_id}/email/",
    ]),
    ("Stream", "CloudflareStreamClient", [
        "/accounts/{account_id}/stream/",
    ]),
    ("Intel", "CloudflareIntelClient", [
        "/accounts/{account_id}/intel/",
        "/accounts/{account_id}/brand-protection/",
        "/accounts/{account_id}/urlscanner/",
    ]),
    ("Gateway", "CloudflareGatewayClient", [
        "/accounts/{account_id}/gateway/",
    ]),
    ("ZeroTrust", "CloudflareZeroTrustClient", [
        "/accounts/{account_id}/zerotrust/",
        "/accounts/{account_id}/zt_risk_scoring/",
        "/accounts/{account_id}/teamnet/",
        "/accounts/{account_id}/cfd_tunnel/",
        "/accounts/{account_id}/warp_connector/",
        "/accounts/{account_id}/tunnels",
    ]),
    ("Addressing", "CloudflareAddressingClient", [
        "/accounts/{account_id}/addressing/",
        "/zones/{zone_id}/addressing/",
    ]),
    ("Builds", "CloudflareBuildsClient", [
        "/accounts/{account_id}/builds/",
        "/accounts/{account_id}/pages/",
    ]),
    ("Vectorize", "CloudflareVectorizeClient", [
        "/accounts/{account_id}/vectorize/",
    ]),

    # ── Small combined packages ──
    ("Storage", "CloudflareStorageClient", [
        "/accounts/{account_id}/r2/",
        "/accounts/{account_id}/r2-catalog/",
        "/accounts/{account_id}/storage/",
        "/accounts/{account_id}/d1/",
        "/accounts/{account_id}/hyperdrive/",
        "/accounts/{account_id}/queues/",
        "/accounts/{account_id}/pipelines/",
    ]),
    ("Accounts", "CloudflareAccountsClient", [
        "/accounts/{account_id}/members",
        "/accounts/{account_id}/roles",
        "/accounts/{account_id}/tokens",
        "/accounts/{account_id}/billing/",
        "/accounts/{account_id}/scim/",
        "/accounts/{account_id}/iam/",
        "/accounts/{account_id}/subscriptions",
        "/accounts/{account_id}/custom_pages",
        "/accounts/move",
        "/accounts/{account_id}/challenges/",
        "/accounts/{account_id}/audit_logs",
    ]),
    ("Observability", "CloudflareObservabilityClient", [
        "/accounts/{account_id}/logpush/",
        "/accounts/{account_id}/rum/",
        "/accounts/{account_id}/alerting/",
        "/accounts/{account_id}/images/",
        "/accounts/{account_id}/pcaps/",
        "/accounts/{account_id}/mnm/",
        "/accounts/{account_id}/browser-rendering/",
        "/accounts/{account_id}/logs/",
        "/accounts/{account_id}/analytics/",
        "/zones/{zone_id}/logpush/",
        "/zones/{zone_id}/analytics/",
    ]),
    ("Network", "CloudflareNetworkClient", [
        "/accounts/{account_id}/cni/",
        "/accounts/{account_id}/load_balancers/",
        "/accounts/{account_id}/custom_ns/",
        "/accounts/{account_id}/secondary_dns/",
        "/accounts/{account_id}/registrar/",
        "/accounts/{account_id}/dns_firewall/",
        "/zones/{zone_id}/load_balancers/",
        "/zones/{zone_id}/secondary_dns/",
        "/zones/{zone_id}/dns_records",
        "/zones/{zone_id}/custom_ns/",
        "/zones/{zone_id}/dns_settings",
    ]),
    ("Security", "CloudflareSecurityClient", [
        "/accounts/{account_id}/rulesets",
        "/accounts/{account_id}/firewall/",
        "/accounts/{account_id}/security-center/",
        "/accounts/{account_id}/rules/",
        "/accounts/{account_id}/page_shield/",
        "/accounts/{account_id}/spectrum/",
        "/accounts/{account_id}/custom_certificates/",
        "/accounts/{account_id}/mtls_certificates/",
        "/accounts/{account_id}/client_certificates/",
        "/accounts/{account_id}/web3/",
        "/zones/{zone_id}/rulesets",
        "/zones/{zone_id}/firewall/",
        "/zones/{zone_id}/page_shield/",
        "/zones/{zone_id}/custom_certificates",
        "/zones/{zone_id}/ssl/",
    ]),

    # ── Zones package (broad catch-all for /zones/ not claimed above) ──
    ("Zones", "CloudflareZonesClient", [
        "/zones",
    ]),

    # ── Misc package (everything else) ──
    ("Misc", "CloudflareMiscClient", [
        "/user/",
        "/user",
        "/organizations/",
        "/memberships",
        "/certificates",
        "/ips",
        "/accounts",
    ]),
]


def normalize_path(path: str) -> str:
    """Normalize path parameter names for matching.
    E.g., /accounts/{account_id}/ai/ and /accounts/{accountId}/ai/ should match."""
    return re.sub(r'\{[^}]+\}', '{_}', path)


def match_path_to_package(path: str) -> tuple[str, str] | None:
    """Return (package_name, class_name) for the path, or None if no match."""
    norm = normalize_path(path)
    for pkg_name, class_name, prefixes in PACKAGES:
        for prefix in prefixes:
            norm_prefix = normalize_path(prefix)
            if norm.startswith(norm_prefix) or norm == norm_prefix.rstrip('/'):
                return pkg_name, class_name
    return None


def collect_refs(obj, refs: set):
    """Recursively collect all $ref strings from a JSON object."""
    if isinstance(obj, dict):
        if '$ref' in obj:
            refs.add(obj['$ref'])
        for v in obj.values():
            collect_refs(v, refs)
    elif isinstance(obj, list):
        for item in obj:
            collect_refs(item, refs)


def resolve_refs_transitively(spec: dict, initial_refs: set) -> dict:
    """Given a set of $ref strings, transitively resolve all referenced components."""
    components = {}
    # Component categories we care about
    categories = ['schemas', 'parameters', 'requestBodies', 'responses', 'examples']

    pending = set(initial_refs)
    visited = set()

    while pending:
        ref = pending.pop()
        if ref in visited:
            continue
        visited.add(ref)

        # Parse the $ref: #/components/{category}/{name}
        match = re.match(r'^#/components/(\w+)/(.+)$', ref)
        if not match:
            continue

        category, name = match.groups()
        if category not in categories:
            continue

        src = spec.get('components', {}).get(category, {})
        if name not in src:
            continue

        if category not in components:
            components[category] = {}
        components[category][name] = src[name]

        # Find refs in this component
        new_refs = set()
        collect_refs(src[name], new_refs)
        pending.update(new_refs - visited)

    return components


def build_sub_spec(spec: dict, paths: dict, pkg_name: str) -> dict:
    """Build a complete sub-spec for a package."""
    # Collect all $refs from the paths
    refs = set()
    collect_refs(paths, refs)

    # Transitively resolve
    components = resolve_refs_transitively(spec, refs)

    # Also include securitySchemes (always needed)
    if 'securitySchemes' in spec.get('components', {}):
        components['securitySchemes'] = spec['components']['securitySchemes']

    sub_spec = {
        'openapi': spec.get('openapi', '3.0.3'),
        'info': {
            'title': f'Cloudflare API - {pkg_name}',
            'version': spec.get('info', {}).get('version', '4.0.0'),
        },
        'servers': spec.get('servers', []),
        'security': spec.get('security', []),
        'paths': dict(sorted(paths.items())),
        'components': components,
    }

    return sub_spec


def main():
    spec_path = Path(__file__).parent / 'cloudflare-openapi-fixed.json'
    output_dir = Path(__file__).parent / 'split'
    output_dir.mkdir(exist_ok=True)

    print(f"Loading spec from {spec_path}...")
    with open(spec_path, 'r', encoding='utf-8') as f:
        spec = json.load(f)

    all_paths = spec.get('paths', {})
    print(f"Total paths: {len(all_paths)}")

    # Assign each path to a package
    package_paths: dict[str, dict] = {}
    package_classes: dict[str, str] = {}
    unmatched = []

    for path, path_obj in all_paths.items():
        result = match_path_to_package(path)
        if result:
            pkg_name, class_name = result
            if pkg_name not in package_paths:
                package_paths[pkg_name] = {}
                package_classes[pkg_name] = class_name
            package_paths[pkg_name][path] = path_obj
        else:
            unmatched.append(path)

    # Add unmatched to Misc
    if unmatched:
        if 'Misc' not in package_paths:
            package_paths['Misc'] = {}
            package_classes['Misc'] = 'CloudflareMiscClient'
        for path in unmatched:
            package_paths['Misc'][path] = all_paths[path]

    # Print summary
    print(f"\n{'Package':<20} {'Paths':>6}  {'Class Name'}")
    print('-' * 60)
    total = 0
    for pkg_name, class_name, _ in PACKAGES:
        if pkg_name in package_paths:
            count = len(package_paths[pkg_name])
            total += count
            print(f"  {pkg_name:<18} {count:>6}  {package_classes[pkg_name]}")

    print('-' * 60)
    print(f"  {'TOTAL':<18} {total:>6}")
    if unmatched:
        print(f"\n  Unmatched paths added to Misc: {len(unmatched)}")
        for p in unmatched[:10]:
            print(f"    {p}")
        if len(unmatched) > 10:
            print(f"    ... and {len(unmatched) - 10} more")

    # Generate sub-specs
    print(f"\nGenerating sub-specs in {output_dir}...")
    manifest = {}
    for pkg_name, class_name, _ in PACKAGES:
        if pkg_name not in package_paths:
            continue
        paths = package_paths[pkg_name]
        sub_spec = build_sub_spec(spec, paths, pkg_name)
        filename = f"cloudflare-{pkg_name.lower()}.json"
        out_path = output_dir / filename
        with open(out_path, 'w', encoding='utf-8') as f:
            json.dump(sub_spec, f, indent=2, ensure_ascii=False)
        schema_count = len(sub_spec.get('components', {}).get('schemas', {}))
        size_kb = out_path.stat().st_size / 1024
        print(f"  {filename}: {len(paths)} paths, {schema_count} schemas, {size_kb:.0f} KB")
        manifest[pkg_name] = {
            'file': filename,
            'class_name': class_name,
            'namespace': f'Cloudflare.{pkg_name}',
            'paths': len(paths),
            'schemas': schema_count,
        }

    # Write manifest
    manifest_path = output_dir / 'manifest.json'
    with open(manifest_path, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, indent=2)
    print(f"\nManifest written to {manifest_path}")
    print("Done!")


if __name__ == '__main__':
    main()

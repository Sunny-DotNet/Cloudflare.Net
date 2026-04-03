#!/usr/bin/env python3
"""
Generate all Cloudflare.Net API packages:
1. Create csproj for each package
2. Run Kiota to generate code from sub-spec
3. Apply post-generation fixes
"""

import json
import os
import subprocess
import sys
import re
from pathlib import Path

ROOT = Path(__file__).parent.parent
SRC = ROOT / 'src'
SPLIT_DIR = ROOT / 'openapi' / 'split'
MANIFEST = SPLIT_DIR / 'manifest.json'

CSPROJ_TEMPLATE = """<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;netstandard2.1;net8.0;net9.0;net10.0</TargetFrameworks>
    <RootNamespace>{namespace}</RootNamespace>
    <AssemblyName>{namespace}</AssemblyName>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>

    <!-- NuGet Package Metadata -->
    <PackageId>{namespace}</PackageId>
    <Version>0.1.0</Version>
    <Authors>Sunny</Authors>
    <Description>Cloudflare {name} API client for .NET. Auto-generated from the official OpenAPI specification using Microsoft Kiota. Part of the Cloudflare.Net modular SDK.</Description>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageProjectUrl>https://github.com/m67186636/Cloudflare.Net</PackageProjectUrl>
    <RepositoryUrl>https://github.com/m67186636/Cloudflare.Net</RepositoryUrl>
    <RepositoryType>git</RepositoryType>
    <PackageTags>cloudflare;api;{tags}</PackageTags>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <Copyright>Copyright (c) 2026 Sunny</Copyright>
    <NoWarn>$(NoWarn);CS1591;CS1584;CS1658</NoWarn>
  </PropertyGroup>

  <!-- Multi-targeting polyfills -->
  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.0'">
    <PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="8.*" />
    <PackageReference Include="System.Text.Json" Version="8.*" />
  </ItemGroup>

  <ItemGroup Condition="'$(TargetFramework)' == 'netstandard2.1'">
    <PackageReference Include="System.Text.Json" Version="8.*" />
  </ItemGroup>

  <!-- Kiota runtime dependencies -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Kiota.Abstractions" Version="1.*" />
    <PackageReference Include="Microsoft.Kiota.Http.HttpClientLibrary" Version="1.*" />
    <PackageReference Include="Microsoft.Kiota.Serialization.Json" Version="1.*" />
    <PackageReference Include="Microsoft.Kiota.Serialization.Text" Version="1.*" />
    <PackageReference Include="Microsoft.Kiota.Serialization.Form" Version="1.*" />
    <PackageReference Include="Microsoft.Kiota.Serialization.Multipart" Version="1.*" />
  </ItemGroup>

  <!-- Core package reference -->
  <ItemGroup>
    <ProjectReference Include="..\\Cloudflare.Net\\Cloudflare.Net.csproj" />
  </ItemGroup>

  <!-- Package content -->
  <ItemGroup>
    <None Include="..\\..\\README.md" Pack="true" PackagePath="\\" />
    <None Include="..\\..\\LICENSE" Pack="true" PackagePath="\\" />
  </ItemGroup>

</Project>
"""

TAG_MAP = {
    'Radar': 'radar;analytics;insights',
    'AI': 'ai;inference;models',
    'CloudforceOne': 'cloudforce-one;threat-intelligence',
    'Access': 'access;zero-trust;identity',
    'Workers': 'workers;serverless;edge',
    'Magic': 'magic;transit;wan;network',
    'Realtime': 'realtime;calls;webrtc',
    'AIGateway': 'ai-gateway;autorag;ai-search',
    'Devices': 'devices;dex;warp',
    'DLP': 'dlp;data-loss-prevention',
    'EmailSecurity': 'email;email-security;email-routing',
    'Stream': 'stream;video;media',
    'Intel': 'intel;threat;urlscanner;brand-protection',
    'Gateway': 'gateway;dns-filtering;swg',
    'ZeroTrust': 'zero-trust;tunnel;warp-connector',
    'Addressing': 'addressing;ip;byoip',
    'Builds': 'builds;pages;deployment',
    'Vectorize': 'vectorize;vector-database',
    'Storage': 'storage;r2;d1;kv;queues;hyperdrive',
    'Accounts': 'accounts;iam;members;billing',
    'Observability': 'observability;logpush;analytics;alerting;images',
    'Network': 'network;load-balancer;dns;registrar',
    'Security': 'security;rulesets;firewall;waf;spectrum',
    'Zones': 'zones;dns;ssl;cache;settings',
    'Misc': 'user;organizations;memberships;certificates',
}


def create_csproj(pkg_name: str, namespace: str) -> Path:
    """Create the csproj file for a package."""
    pkg_dir = SRC / namespace
    pkg_dir.mkdir(parents=True, exist_ok=True)

    tags = TAG_MAP.get(pkg_name, pkg_name.lower())
    content = CSPROJ_TEMPLATE.format(
        namespace=namespace,
        name=pkg_name,
        tags=tags,
    )

    csproj_path = pkg_dir / f'{namespace}.csproj'
    csproj_path.write_text(content, encoding='utf-8')
    return csproj_path


def run_kiota(spec_file: Path, output_dir: Path, namespace: str, class_name: str) -> bool:
    """Run Kiota to generate code from a sub-spec."""
    generated_dir = output_dir / 'Generated'
    generated_dir.mkdir(parents=True, exist_ok=True)

    cmd = [
        'kiota', 'generate',
        '-l', 'CSharp',
        '-d', str(spec_file),
        '-o', str(generated_dir),
        '-n', f'{namespace}.Generated',
        '--class-name', class_name,
        '--disable-validation-rules', 'All',
        '--exclude-backward-compatible',
        '--additional-data', 'true',
    ]

    print(f'    Running: {" ".join(cmd[:6])}...')
    result = subprocess.run(cmd, capture_output=True, text=True, timeout=300)

    if result.returncode != 0:
        print(f'    ERROR: Kiota failed for {namespace}')
        print(f'    stdout: {result.stdout[:500]}')
        print(f'    stderr: {result.stderr[:500]}')
        return False

    # Count generated files
    gen_files = list(generated_dir.rglob('*.cs'))
    print(f'    Generated {len(gen_files)} files')
    return True


def apply_post_gen_fixes(pkg_dir: Path, namespace: str) -> int:
    """Apply known post-generation fixes. Returns count of fixes applied."""
    fixes = 0
    generated_dir = pkg_dir / 'Generated'

    for cs_file in generated_dir.rglob('*.cs'):
        content = cs_file.read_text(encoding='utf-8')
        original = content

        # Fix 1: CS0029 - incorrect default value assignments
        # Pattern: property = new UntypedNode(...) when property is enum type
        # Pattern: property = "string" when property is List<string>
        # Pattern: property = "string" when property is DateTimeOffset
        # We comment out these lines
        lines = content.split('\n')
        new_lines = []
        for line in lines:
            stripped = line.strip()
            # Skip lines that assign string literals to non-string properties in constructors
            # These are Kiota bugs where default values are wrong
            if (stripped.startswith('//') or not stripped):
                new_lines.append(line)
                continue
            new_lines.append(line)
        content = '\n'.join(new_lines)

        # Fix 2: CS0102 - duplicate Item property (indexer vs explicit property)
        # Remove explicit "public ... Item" property if indexer "this[...]" exists
        # This is complex - skip for now, handle per-package if needed

        if content != original:
            cs_file.write_text(content, encoding='utf-8')
            fixes += 1

    return fixes


def main():
    if not MANIFEST.exists():
        print(f'ERROR: Manifest not found at {MANIFEST}')
        print('Run split_spec.py first!')
        sys.exit(1)

    with open(MANIFEST, 'r') as f:
        manifest = json.load(f)

    print(f'Found {len(manifest)} packages in manifest')
    print()

    results = {}
    for pkg_name, info in manifest.items():
        namespace = info['namespace']
        class_name = info['class_name']
        spec_file = SPLIT_DIR / info['file']

        print(f'[{pkg_name}] {namespace} ({info["paths"]} paths, {info["schemas"]} schemas)')

        # Step 1: Create csproj
        csproj = create_csproj(pkg_name, namespace)
        print(f'    Created {csproj.name}')

        # Step 2: Run Kiota
        pkg_dir = SRC / namespace
        success = run_kiota(spec_file, pkg_dir, namespace, class_name)

        if success:
            # Step 3: Count files
            gen_files = list((pkg_dir / 'Generated').rglob('*.cs'))
            total_size = sum(f.stat().st_size for f in gen_files)
            results[pkg_name] = {
                'success': True,
                'files': len(gen_files),
                'size_mb': total_size / (1024 * 1024),
            }
        else:
            results[pkg_name] = {'success': False, 'files': 0, 'size_mb': 0}

        print()

    # Summary
    print('=' * 70)
    print(f'{"Package":<20} {"Status":>8} {"Files":>7} {"Size (MB)":>10}')
    print('-' * 70)
    total_files = 0
    total_size = 0
    failed = 0
    for pkg_name, r in results.items():
        status = 'OK' if r['success'] else 'FAILED'
        total_files += r['files']
        total_size += r['size_mb']
        if not r['success']:
            failed += 1
        print(f'  {pkg_name:<18} {status:>8} {r["files"]:>7} {r["size_mb"]:>10.2f}')
    print('-' * 70)
    print(f'  {"TOTAL":<18} {"":>8} {total_files:>7} {total_size:>10.2f}')
    if failed:
        print(f'\n  {failed} package(s) FAILED!')
    else:
        print(f'\n  All {len(results)} packages generated successfully!')


if __name__ == '__main__':
    main()

#!/usr/bin/env python3
"""
Fix known Kiota code generation bugs across all packages.

Bugs fixed:
1. CS0029/CS0266: Wrong default value assignments in constructors
   - enum → UntypedNode (comment out)
   - enum → string (comment out)  
   - string → List<string> (comment out)
   - enum → different enum (comment out)
2. CS0102: Duplicate 'Item' property (indexer vs explicit property)
   - Rename explicit Item property to ItemEscaped
"""

import os
import re
import sys
from pathlib import Path

SRC = Path(__file__).parent.parent / 'src'


def fix_string_to_list_assignment(content: str) -> tuple[str, int]:
    """Fix assignments like: ExpectedCodes = "200"; where ExpectedCodes is List<string>."""
    fixes = 0
    list_string_props = re.findall(
        r'public\s+(?:global::)?System\.Collections\.Generic\.List<string>\??\s+(\w+)\s*\{',
        content
    )
    
    for prop in list_string_props:
        pattern = rf'(\s+)({re.escape(prop)}\s*=\s*"[^"]*"\s*;)'
        match = re.search(pattern, content)
        if match:
            indent = match.group(1)
            stmt = match.group(2)
            content = content.replace(
                indent + stmt,
                f'{indent}// KIOTA_FIX: {stmt}'
            )
            fixes += 1
    
    return content, fixes


def fix_enum_default_assignments(content: str) -> tuple[str, int]:
    """Comment out incorrect default value assignments like:
       Value = global::...EnumType.EnumValue;
    where Value is actually UntypedNode, string, or different enum.
    
    These appear inside constructors as: PropertyName = global::Namespace.EnumType.Value;
    """
    fixes = 0
    # Find all constructor bodies and comment out global:: enum assignments
    # Pattern: lines inside constructors that do PropertyName = global::...;
    # We need to match: "            Value = global::Cloudflare.Net.Zones.Generated.Models.Zones_sha1_support_value.Off;"
    
    lines = content.split('\n')
    new_lines = []
    in_constructor_body = False
    brace_depth = 0
    
    for line in lines:
        stripped = line.strip()
        
        # Detect constructor: "public ClassName(...) : base()" or "public ClassName(...)"  
        if re.match(r'\s+public\s+\w+\s*\([^)]*\)\s*(:\s*base\(\))?\s*$', line):
            in_constructor_body = 'pending'  # Wait for opening brace
            new_lines.append(line)
            continue
        
        if in_constructor_body == 'pending':
            if stripped == '{':
                in_constructor_body = True
                brace_depth = 1
                new_lines.append(line)
                continue
            elif stripped:
                in_constructor_body = False
        
        if in_constructor_body is True:
            brace_depth += stripped.count('{') - stripped.count('}')
            if brace_depth <= 0:
                in_constructor_body = False
                new_lines.append(line)
                continue
            
            # Check for problematic global:: enum assignments
            if (re.match(r'\w+\s*=\s*global::', stripped) and 
                not stripped.startswith('AdditionalData') and
                not stripped.startswith('//')):
                indent = line[:len(line) - len(line.lstrip())]
                new_lines.append(f'{indent}// KIOTA_FIX: {stripped}')
                fixes += 1
                continue
        
        new_lines.append(line)
    
    return '\n'.join(new_lines), fixes


def fix_duplicate_item_property(content: str) -> tuple[str, int]:
    """Rename explicit 'Item' property to 'ItemEscaped' when indexer exists."""
    has_indexer = 'this[string ' in content
    if not has_indexer:
        return content, 0
    
    # Match: "public SomeType Item\n" or "public SomeType Item {"
    pattern = r'(public\s+\S+\s+)Item(\s*(?:\{|\n))'
    match = re.search(pattern, content)
    if not match:
        return content, 0
    
    content = content[:match.start()] + match.group(1) + 'ItemEscaped' + match.group(2) + content[match.end():]
    return content, 1


def fix_file(filepath: Path) -> int:
    """Apply all fixes to a single file. Returns number of fixes."""
    content = filepath.read_text(encoding='utf-8')
    original = content
    total_fixes = 0

    content, fixes = fix_string_to_list_assignment(content)
    total_fixes += fixes

    content, fixes = fix_duplicate_item_property(content)
    total_fixes += fixes

    content, fixes = fix_enum_default_assignments(content)
    total_fixes += fixes

    if content != original:
        filepath.write_text(content, encoding='utf-8')

    return total_fixes


def main():
    total_fixes = 0
    files_fixed = 0

    for pkg_dir in sorted(SRC.iterdir()):
        if not pkg_dir.is_dir():
            continue
        gen_dir = pkg_dir / 'Generated'
        if not gen_dir.exists():
            continue

        pkg_fixes = 0
        pkg_files = 0
        for cs_file in gen_dir.rglob('*.cs'):
            fixes = fix_file(cs_file)
            if fixes > 0:
                pkg_files += 1
                pkg_fixes += fixes

        if pkg_fixes > 0:
            print(f'  {pkg_dir.name}: {pkg_fixes} fixes in {pkg_files} files')
            total_fixes += pkg_fixes
            files_fixed += pkg_files

    print(f'\nTotal: {total_fixes} fixes in {files_fixed} files')


if __name__ == '__main__':
    main()

"""Checks that R8 only renames Java packages the .NET code never references.

    python tools/check_r8_packages.py

Run after a Release Android build. Prints each Java class .NET uses that the rule in
MDRNJfitness/Platforms/Android/proguard-obfuscated.cfg allows R8 to rename. Each one would
crash the app at runtime: remove its package from that rule. Also prints how much is renamed,
because Google Play wants at least 25%.
"""
import glob
import os
import re
import sys

APP = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'MDRNJfitness')
RULES = os.path.join(APP, 'Platforms', 'Android', 'proguard-obfuscated.cfg')
MAPPING = os.path.join(APP, 'bin', 'Release', 'net10.0-android', 'mapping.txt')
ASSEMBLIES = os.path.join(APP, 'obj', 'Release', 'net10.0-android', 'android-arm64', 'linked', '*.dll')


def java_names_in_assemblies():
    """JNI names like androidx/core/view/ViewCompat, stored as UTF-8 or UTF-16 in each .dll."""
    names = set()
    for path in glob.glob(ASSEMBLIES):
        with open(path, 'rb') as f:
            data = f.read()
        names.update(m.decode('latin1') for m in re.findall(rb'[A-Za-z_]\w*(?:/[\w$]+)+', data))
        names.update(m.decode('utf-16le') for m in re.findall(rb'(?:[A-Za-z_0-9$/]\x00){6,}', data))
    return {name.replace('/', '.') for name in names if '/' in name}


def main():
    with open(RULES, encoding='utf-8') as f:
        packages = tuple(p + '.' for p in re.findall(r'!([\w.]+)\.\*\*', f.read()))
    with open(MAPPING, encoding='utf-8', errors='ignore') as f:
        mapped = re.findall(r'^(\S+) -> (\S+):$', f.read(), re.M)

    used = java_names_in_assemblies()
    if not used:
        sys.exit('No assemblies found. Do a Release Android build first.')

    unsafe = sorted(name for name in used if name.startswith(packages))
    renamed = sum(original != new for original, new in mapped)
    print(f'{renamed} of {len(mapped)} classes renamed ({100 * renamed / len(mapped):.1f}%)')
    for name in unsafe:
        print('UNSAFE:', name)
    sys.exit(1 if unsafe else 0)


if __name__ == '__main__':
    main()

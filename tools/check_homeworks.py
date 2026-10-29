"""CLI integration checks. Build both Desktop solutions before running this file."""
import json
import subprocess
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TEMP_ROOT = ROOT / 'artifacts' / 'cli-tests'
TEMP_ROOT.mkdir(parents=True, exist_ok=True)
passed = 0

def check(condition, description):
    global passed
    if not condition:
        raise AssertionError(description)
    passed += 1

def run(hw, args, input_text=None):
    app = ROOT / f'HomeWork{hw}/prjPasswordCreator/cnsPasswordCreator/bin/Debug/net10.0/cnsPasswordCreator.dll'
    return subprocess.run(['dotnet', str(app), *args], input=input_text, capture_output=True,
                          text=True, encoding='utf-8', errors='replace', timeout=45)

for hw in (3, 4):
    tests = ROOT / f'HomeWork{hw}/prjPasswordCreator/PasswordCreator.Tests/bin/Debug/net10.0/PasswordCreator.Tests.dll'
    result = subprocess.run(['dotnet', str(tests)], capture_output=True, text=True, encoding='utf-8', errors='replace', timeout=45)
    check(result.returncode == 0, result.stderr + result.stdout)
    print(result.stdout.strip(), flush=True)

    result = run(hw, ['--length', '24', '--count', '3', '--groups', 'luds'])
    check(result.returncode == 0, 'standard CLI exit status')
    passwords = result.stdout.splitlines()
    check(len(passwords) == 3 and all(len(p) == 24 for p in passwords), 'standard CLI batch')
    check(all(any(c.islower() for c in p) and any(c.isupper() for c in p) and any(c.isdigit() for c in p) for p in passwords), 'CLI group coverage')

    result = run(hw, ['--length', '1', '--count', '100', '--groups', 'd'])
    check(result.returncode == 0 and len(result.stdout.splitlines()) == 100 and all(p.isdigit() and len(p) == 1 for p in result.stdout.splitlines()), 'CLI digit-only minimum')
    for args in (['--length', 'abc'], ['--count', '0'], ['--length'], ['--groups', 'x'], ['--groups', ''], ['--unknown']):
        result = run(hw, args)
        check(result.returncode == 1 and not result.stdout and result.stderr, 'CLI rejects invalid arguments ' + str(args))
    result = run(hw, ['--help'])
    check(result.returncode == 0 and '--length' in result.stdout, 'CLI help')
    interactive = '12\ny\ny\ny\ny\n2\n' if hw == 3 else '1\n6\ny\nn\n3\n'
    check(run(hw, [], interactive).returncode == 0, 'interactive CLI')

    with tempfile.TemporaryDirectory(prefix='passwordcreator-cli-', dir=TEMP_ROOT) as directory:
        check(Path(directory).resolve().is_relative_to(TEMP_ROOT.resolve()), 'temporary directory is inside workspace')
        destination = Path(directory) / 'passwords.txt'
        result = run(hw, ['--count', '2', '--output', str(destination)])
        check(result.returncode == 0 and destination.read_text(encoding='utf-8').splitlines() == result.stdout.splitlines(), 'UTF-8 file export: ' + result.stderr)
        result = run(hw, ['--output', str(Path(directory) / 'missing' / 'out.txt')])
        check(result.returncode == 1 and result.stderr, 'export I/O failure')
        if hw == 4:
            settings = Path(directory) / 'settings.json'
            settings.write_text(json.dumps({'Mode': 1, 'Length': 6, 'Count': 2}), encoding='utf-8')
            result = run(hw, ['--settings', str(settings), '--count', '3'])
            check(result.returncode == 0 and len(result.stdout.splitlines()) == 3 and all(p.isdigit() and len(p) == 6 for p in result.stdout.splitlines()), 'JSON settings and CLI override order')
            settings.write_text('{bad-json', encoding='utf-8')
            check(run(hw, ['--settings', str(settings)]).returncode == 1, 'invalid settings JSON')

    # Parse all authored XAML, project, manifest, SVG and plist files, excluding outputs.
    for path in (ROOT / f'HomeWork{hw}/prjPasswordCreator').rglob('*'):
        if any(part in ('obj', 'bin') for part in path.parts) or not path.is_file():
            continue
        if path.suffix in ('.xaml', '.csproj', '.props', '.slnx', '.xml', '.appxmanifest', '.manifest', '.svg', '.plist'):
            ET.parse(path)
            check(True, 'XML well-formed: ' + str(path.relative_to(ROOT)))

for mode, extra in [('Pin', ['--length', '6']), ('Passphrase', ['--words', '10']), ('Custom', ['--alphabet', 'abcd0123', '--length', '8', '--unique'])]:
    result = run(4, ['--mode', mode, '--count', '3', *extra])
    check(result.returncode == 0 and len(result.stdout.splitlines()) == 3, 'extended CLI mode ' + mode)
    for password in result.stdout.splitlines():
        if mode == 'Pin': check(password.isdigit() and len(password) == 6, 'PIN format')
        if mode == 'Passphrase': check(len(password.split('-')) == 10, 'phrase format')
        if mode == 'Custom': check(len(password) == len(set(password)) == 8 and set(password) == set('abcd0123'), 'custom alphabet and uniqueness')
for args in (['--mode', 'wrong'], ['--mode', 'Pin', '--unique', '--length', '11'], ['--mode', 'Passphrase', '--words', '3']):
    check(run(4, args).returncode == 1, 'extended CLI validation')
print(f'PASS: {passed} CLI and XML integration checks.', flush=True)

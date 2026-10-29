"""Create two independent source archives, without build/cache output."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

ROOT = Path(__file__).resolve().parents[1]
destination = ROOT / 'artifacts'
destination.mkdir(exist_ok=True)
for hw in (3, 4):
    source = ROOT / f'HomeWork{hw}'
    target = destination / f'HomeWork{hw}-prjPasswordCreator.zip'
    paths = [path for path in source.rglob('*') if path.is_file() and not any(part in ('bin', 'obj', '.vs') for part in path.relative_to(source).parts)]
    with ZipFile(target, 'w', ZIP_DEFLATED) as archive:
        for path in sorted(paths):
            archive.write(path, path.relative_to(ROOT).as_posix())
        archive.write(ROOT / 'assignment-requirements.txt', f'HomeWork{hw}/assignment-requirements.txt')
    with ZipFile(target) as archive:
        assert archive.testzip() is None
        assert all(name.startswith(f'HomeWork{hw}/') for name in archive.namelist())
    print(f'{target.name}: {len(paths) + 1} files, {target.stat().st_size} bytes', flush=True)

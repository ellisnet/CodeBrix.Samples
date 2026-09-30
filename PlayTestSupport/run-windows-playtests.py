#!/usr/bin/env python3
"""Discover .PlayTests.csproj files and run the Windows preview validation matrix."""
import argparse
from datetime import datetime
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import time


SAMPLES = Path(__file__).resolve().parents[1]
CONFIGURATIONS = (
    ("headless", "light", "landscape"),
    ("headless", "light", "portrait"),
    ("headed", "dark", "landscape"),
    ("headed", "dark", "portrait"),
)


def discover(root):
    return sorted(path for path in root.rglob("*.PlayTests.csproj")
                  if not {"bin", "obj", ".git"}.intersection(path.relative_to(root).parts))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--platform-repo", type=Path, default=SAMPLES.parent / "CodeBrix.Platform")
    parser.add_argument("--output", type=Path, default=SAMPLES / "PlayTestSupport" / "TestResults")
    parser.add_argument("--suites", nargs="+", help="Application names, for example PlayTestDemo WikipediaPublisher")
    parser.add_argument("--version", help="Override the projects' default local preview package version")
    parser.add_argument("--configuration", choices=["-".join(c) for c in CONFIGURATIONS])
    parser.add_argument("--no-build", action="store_true")
    parser.add_argument("--slowmo", type=float, help="Override action delay in milliseconds (default: unset)")
    args = parser.parse_args()
    if os.name != "nt":
        parser.error("This runner validates the Windows host; run it on Windows.")
    sys.stdout.reconfigure(errors="backslashreplace")
    platform = args.platform_repo.resolve()
    if not platform.is_dir():
        parser.error(f"Platform repository does not exist: {platform}")
    projects = [(SAMPLES, p) for p in discover(SAMPLES)] + [(platform, p) for p in discover(platform / "samples")]
    names = {p.stem.removesuffix(".PlayTests") for _, p in projects}
    if args.suites:
        missing = set(args.suites) - names
        if missing:
            parser.error("No PlayTests project for: " + ", ".join(sorted(missing)))
        projects = [(r, p) for r, p in projects if p.stem.removesuffix(".PlayTests") in args.suites]
    if not projects:
        parser.error("No .PlayTests.csproj files discovered.")
    output = args.output.resolve() / datetime.now().strftime("%Y%m%d-%H%M%S-%f")
    output.mkdir(parents=True)
    print(f"Discovered {len(projects)} PlayTests projects. Results: {output}", flush=True)
    results = []
    for mode, theme, orientation in CONFIGURATIONS:
        label = f"{mode}-{theme}-{orientation}"
        if args.configuration and label != args.configuration:
            continue
        env = os.environ.copy()
        env["MSBUILDDISABLENODEREUSE"] = "1"
        env["CODEBRIX_PLAYTEST_HEADED"] = "1" if mode == "headed" else "0"
        env["CODEBRIX_PLAYTEST_THEME"] = theme
        env["CODEBRIX_PLAYTEST_ORIENTATION"] = orientation
        for variable in ("CODEBRIX_PLAYTEST_SLOWMO", "SDL_VIDEODRIVER", "SDL_RENDER_DRIVER"):
            env.pop(variable, None)
        if mode == "headed":
            env["SDL_VIDEODRIVER"] = "windows"
        if args.slowmo is not None:
            env["CODEBRIX_PLAYTEST_SLOWMO"] = str(args.slowmo)
        for repo, project in projects:
            suite = project.stem.removesuffix(".PlayTests")
            command = ["dotnet", "test", "--project", str(project), "-c", "Release"]
            if args.version:
                command.append(f"-p:PlayTestPackageVersion={args.version}")
            if args.no_build:
                command += ["--no-build", "--no-restore"]
            log = output / f"{suite}-{label}.log"
            print(f"START {suite} {label}", flush=True)
            started = time.monotonic()
            with log.open("w", encoding="utf-8") as stream:
                stream.write("COMMAND: " + subprocess.list2cmdline(command) + "\n")
                stream.flush()
                process = subprocess.Popen(command, cwd=repo, env=env, stdout=stream,
                                           stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NO_WINDOW)
                try:
                    code = process.wait(timeout=900)
                except subprocess.TimeoutExpired:
                    subprocess.run(["taskkill", "/PID", str(process.pid), "/T", "/F"],
                                   stdout=stream, stderr=subprocess.STDOUT,
                                   creationflags=subprocess.CREATE_NO_WINDOW, check=False)
                    process.wait()
                    code = 124
            content = log.read_text(encoding="utf-8", errors="replace")
            counts = {key: int(value) for key, value in re.findall(
                r"^\s*(total|failed|succeeded|skipped):\s*(\d+)", content, re.MULTILINE)}
            passed = (code == 0 and counts.get("total", 0) > 0
                      and counts.get("succeeded") == counts["total"]
                      and counts.get("failed") == 0 and counts.get("skipped") == 0)
            result = dict(suite=suite, project=str(project), configuration=label, exit_code=code,
                          passed=passed, counts=counts, seconds=round(time.monotonic() - started, 1),
                          log=str(log), slowmo=args.slowmo)
            results.append(result)
            (output / "summary.json").write_text(json.dumps(results, indent=2), encoding="utf-8")
            print(json.dumps(result), flush=True)
            if not passed:
                print(content[-12000:], flush=True)
    print(f"Passed {sum(r['passed'] for r in results)}/{len(results)} suite runs. Summary: {output / 'summary.json'}", flush=True)
    return 0 if all(r["passed"] for r in results) else 1


if __name__ == "__main__":
    raise SystemExit(main())

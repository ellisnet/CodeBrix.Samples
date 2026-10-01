#!/usr/bin/env python3
"""Run all discovered sample PlayTests on macOS, including their Cocoa previews."""
from pathlib import Path
import runpy

if __name__ == "__main__":
    runner = runpy.run_path(str(Path(__file__).with_name("run-windows-playtests.py")))
    raise SystemExit(runner["main"]("macos"))

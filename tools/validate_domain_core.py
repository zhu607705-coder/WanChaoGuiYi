#!/usr/bin/env python3
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
DOMAIN_CORE = ROOT / "domain-core" / "src"
DOMAIN = DOMAIN_CORE / "Domain"
HEADLESS_CSPROJ = ROOT / "tools" / "headless_runner" / "WanChaoGuiYiHeadless" / "WanChaoGuiYiHeadless.csproj"

FORBIDDEN_DOMAIN_TOKENS = [
    "using UnityEngine",
    "MonoBehaviour",
    "SerializeField",
    "GetComponent",
    "gameObject",
    "Mathf.",
    "MapGraph ",
    "MapGraph)",
]

def fail(message: str) -> None:
    print(f"[domain-core] ERROR: {message}")
    sys.exit(1)


def validate_domain_core_folder() -> None:
    if not DOMAIN_CORE.exists():
        fail(f"Domain core folder missing: {DOMAIN_CORE.relative_to(ROOT)}")
    if not DOMAIN.exists():
        fail(f"Domain folder missing: {DOMAIN.relative_to(ROOT)}")

    source_files = sorted(DOMAIN_CORE.rglob("*.cs"))
    if len(source_files) < 20:
        fail(f"Expected migrated C# domain/core files under domain-core/src, got {len(source_files)}")

    for path in source_files:
        text = path.read_text(encoding="utf-8")
        rel = path.relative_to(ROOT)
        for token in FORBIDDEN_DOMAIN_TOKENS:
            if token in text:
                fail(f"Forbidden token '{token}' in {rel}")


def validate_headless_project_links() -> None:
    if not HEADLESS_CSPROJ.exists():
        fail(f"Headless csproj missing: {HEADLESS_CSPROJ.relative_to(ROOT)}")

    text = HEADLESS_CSPROJ.read_text(encoding="utf-8")
    if "My project/Assets/Scripts" in text or "My project\\Assets\\Scripts" in text:
        fail("Headless csproj still links C# source from My project/Assets/Scripts")
    if "../../../domain-core/src/" not in text:
        fail("Headless csproj does not link migrated domain-core/src files")


def main() -> None:
    validate_domain_core_folder()
    validate_headless_project_links()
    print("[domain-core] OK: domain-core/src is Unity-free and headless links migrated C# sources.")


if __name__ == "__main__":
    main()

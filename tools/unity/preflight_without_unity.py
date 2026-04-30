#!/usr/bin/env python3
import json
import os
import sys


ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
PROJECT = os.path.join(ROOT, "WanChaoGuiYi")
ASSETS = os.path.join(PROJECT, "Assets")
DATA = os.path.join(ASSETS, "Data")


def fail(message):
    print("[unity-preflight] FAIL: " + message)
    return 1


def load_json(path):
    with open(path, "r", encoding="utf-8") as handle:
        return json.load(handle)


def require_file(path):
    if not os.path.isfile(path):
        return fail("missing file: " + os.path.relpath(path, ROOT))
    return 0


def require_json_table(name):
    path = os.path.join(DATA, name + ".json")
    code = require_file(path)
    if code != 0:
        return code

    payload = load_json(path)
    items = payload.get("items")
    if not isinstance(items, list) or not items:
        return fail(name + ".json has no non-empty items array")
    return 0


def main():
    required_tables = [
        "emperors",
        "portraits",
        "regions",
        "map_region_shapes",
        "historical_layers",
        "policies",
        "events",
        "chronicle_events",
        "talents",
        "units",
        "technologies",
        "victory_conditions",
        "generals",
        "buildings",
    ]

    for table in required_tables:
        code = require_json_table(table)
        if code != 0:
            return code

    regions = load_json(os.path.join(DATA, "regions.json"))["items"]
    shapes = load_json(os.path.join(DATA, "map_region_shapes.json"))["items"]
    region_ids = {item.get("id") for item in regions}
    shaped_region_ids = {item.get("regionId") for item in shapes}
    missing_shapes = sorted(region_ids - shaped_region_ids)
    if missing_shapes:
        return fail("regions without map_region_shapes: " + ", ".join(missing_shapes[:10]))

    required_files = [
        os.path.join(ASSETS, "Scripts", "WanChaoGuiYi.Runtime.asmdef"),
        os.path.join(ASSETS, "Tests", "PlayMode", "WanChaoGuiYi.PlayModeTests.asmdef"),
        os.path.join(ASSETS, "Tests", "PlayMode", "GameManagerPlayModeSmokeTests.cs"),
        os.path.join(ROOT, "tools", "unity", "run_playmode_tests.sh"),
        os.path.join(ROOT, "tools", "verify_unity_handoff.sh"),
    ]
    for path in required_files:
        code = require_file(path)
        if code != 0:
            return code

    manifest = load_json(os.path.join(PROJECT, "Packages", "manifest.json"))
    dependencies = manifest.get("dependencies", {})
    for package_name in ["com.unity.test-framework", "com.unity.ugui"]:
        if package_name not in dependencies:
            return fail("missing Unity package dependency: " + package_name)

    playmode_asmdef = load_json(os.path.join(ASSETS, "Tests", "PlayMode", "WanChaoGuiYi.PlayModeTests.asmdef"))
    if "WanChaoGuiYi.Runtime" not in playmode_asmdef.get("references", []):
        return fail("PlayMode asmdef does not reference WanChaoGuiYi.Runtime")

    print("[unity-preflight] OK: data tables, map shapes, asmdefs, packages, and Unity handoff entrypoints are present.")
    return 0


if __name__ == "__main__":
    sys.exit(main())

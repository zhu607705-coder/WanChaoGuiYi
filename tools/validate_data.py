#!/usr/bin/env python3
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "WanChaoGuiYi" / "Assets" / "Data"


def load(name):
    path = DATA / name
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def fail(message):
    raise SystemExit(f"DATA VALIDATION FAILED: {message}")


def main():
    emperors = load("emperors.json")["items"]
    portraits = load("portraits.json")["items"]
    regions = load("regions.json")["items"]
    historical_layers = load("historical_layers.json")["items"]
    policies = load("policies.json")["items"]
    events = load("events.json")["items"]
    chronicle_events = load("chronicle_events.json")["items"]
    talents = load("talents.json")["items"]
    units = load("units.json")["items"]
    technologies = load("technologies.json")["items"]
    victories = load("victory_conditions.json")["items"]

    if len(emperors) < 8:
        fail(f"MVP requires at least 8 emperors, got {len(emperors)}")

    emperor_ids = assert_unique_ids(emperors, "emperor")
    portrait_ids = assert_unique_ids(portraits, "portrait")
    region_ids = assert_unique_ids(regions, "region")
    historical_layer_ids = assert_unique_ids(historical_layers, "historical layer")
    policy_ids = assert_unique_ids(policies, "policy")
    assert_unique_ids(events, "event")
    chronicle_event_ids = assert_unique_ids(chronicle_events, "chronicle event")
    assert_unique_ids(talents, "talent")
    unit_ids = assert_unique_ids(units, "unit")
    technology_ids = assert_unique_ids(technologies, "technology")
    assert_unique_ids(victories, "victory condition")

    if not portrait_ids:
        fail("portrait table is empty")

    portrait_emperor_ids = set()
    for portrait in portraits:
        emperor_id = portrait.get("emperorId")
        if emperor_id not in emperor_ids:
            fail(f"portrait {portrait['id']} references missing emperor {emperor_id}")
        portrait_emperor_ids.add(emperor_id)
        asset_path = portrait.get("assetPath", "")
        if not asset_path.startswith("Assets/Art/Portraits/") or not asset_path.endswith(".png"):
            fail(f"portrait {portrait['id']} has invalid assetPath {asset_path}")
        if not portrait.get("prompt"):
            fail(f"portrait {portrait['id']} missing generation prompt")

    missing_portraits = emperor_ids - portrait_emperor_ids
    if missing_portraits:
        # 非阻塞警告：新增帝皇可能尚未有立绘
        print(f"WARNING: missing portraits for emperors: {sorted(missing_portraits)}")

    for region in regions:
        for neighbor in region.get("neighbors", []):
            if neighbor not in region_ids:
                fail(f"region {region['id']} references missing neighbor {neighbor}")

    region_by_id = {region["id"]: region for region in regions}
    for region in regions:
        for neighbor in region.get("neighbors", []):
            neighbor_edges = set(region_by_id[neighbor].get("neighbors", []))
            if region["id"] not in neighbor_edges:
                fail(f"region edge {region['id']} -> {neighbor} is not bidirectional")

    historical_layer_region_ids = set()
    for layer in historical_layers:
        region_id = layer.get("regionId")
        if region_id not in region_ids:
            fail(f"historical layer {layer['id']} references missing region {region_id}")
        if region_id in historical_layer_region_ids:
            fail(f"region {region_id} has duplicate historical layers")
        historical_layer_region_ids.add(region_id)
        for technology_id in layer.get("techAffinities", []):
            if technology_id not in technology_ids:
                fail(f"historical layer {layer['id']} references missing technology {technology_id}")

    missing_layers = region_ids - historical_layer_region_ids
    if missing_layers:
        fail(f"missing historical layers for regions: {sorted(missing_layers)}")

    for emperor in emperors:
        required_fields = ["civilization", "legitimacyTypes", "mapScope", "globalMechanicTag"]
        for field in required_fields:
            if not emperor.get(field):
                fail(f"emperor {emperor['id']} missing required global-ready field {field}")

        for policy_id in emperor.get("preferredPolicies", []):
            if policy_id not in policy_ids:
                fail(f"emperor {emperor['id']} references missing policy {policy_id}")

    for technology in technologies:
        for prerequisite in technology.get("prerequisites", []):
            if prerequisite not in technology_ids:
                fail(f"technology {technology['id']} references missing prerequisite {prerequisite}")

        unlocks = technology.get("unlocks", {})
        for unit_id in unlocks.get("units", []):
            if unit_id not in unit_ids:
                fail(f"technology {technology['id']} unlocks missing unit {unit_id}")
        for policy_id in unlocks.get("policies", []):
            if policy_id not in policy_ids:
                fail(f"technology {technology['id']} unlocks missing policy {policy_id}")
        for event_id in unlocks.get("events", []):
            if event_id not in chronicle_event_ids:
                fail(f"technology {technology['id']} unlocks missing chronicle event {event_id}")

    for chronicle_event in chronicle_events:
        for technology_id in chronicle_event.get("requiredTechs", []):
            if technology_id not in technology_ids:
                fail(f"chronicle event {chronicle_event['id']} requires missing technology {technology_id}")
        if not chronicle_event.get("choices"):
            fail(f"chronicle event {chronicle_event['id']} has no choices")

    print("DATA VALIDATION PASSED")
    print(
        " ".join(
            [
                f"emperors={len(emperors)}",
                f"portraits={len(portraits)}",
                f"regions={len(regions)}",
                f"historical_layers={len(historical_layers)}",
                f"policies={len(policies)}",
                f"units={len(units)}",
                f"technologies={len(technologies)}",
                f"chronicle_events={len(chronicle_events)}",
            ]
        )
    )


def assert_unique_ids(items, label):
    ids = set()
    for item in items:
        item_id = item.get("id")
        if not item_id:
            fail(f"{label} missing id")
        if item_id in ids:
            fail(f"duplicate {label} id {item_id}")
        ids.add(item_id)
    return ids


if __name__ == "__main__":
    main()

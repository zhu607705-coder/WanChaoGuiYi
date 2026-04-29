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
    map_region_shapes = load("map_region_shapes.json")["items"]
    historical_layers = load("historical_layers.json")["items"]
    policies = load("policies.json")["items"]
    events = load("events.json")["items"]
    chronicle_events = load("chronicle_events.json")["items"]
    talents = load("talents.json")["items"]
    units = load("units.json")["items"]
    technologies = load("technologies.json")["items"]
    victories = load("victory_conditions.json")["items"]
    generals = load("generals.json")["items"]
    buildings = load("buildings.json")["items"]

    if len(emperors) < 8:
        fail(f"MVP requires at least 8 emperors, got {len(emperors)}")

    emperor_ids = assert_unique_ids(emperors, "emperor")
    portrait_ids = assert_unique_ids(portraits, "portrait")
    region_ids = assert_unique_ids(regions, "region")
    assert_unique_ids(map_region_shapes, "map region shape")
    historical_layer_ids = assert_unique_ids(historical_layers, "historical layer")
    policy_ids = assert_unique_ids(policies, "policy")
    assert_unique_ids(events, "event")
    chronicle_event_ids = assert_unique_ids(chronicle_events, "chronicle event")
    assert_unique_ids(talents, "talent")
    unit_ids = assert_unique_ids(units, "unit")
    technology_ids = assert_unique_ids(technologies, "technology")
    assert_unique_ids(victories, "victory condition")
    general_ids = assert_unique_ids(generals, "general")
    building_ids = assert_unique_ids(buildings, "building")

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
        if not (ROOT / "WanChaoGuiYi" / asset_path).exists():
            fail(f"portrait {portrait['id']} assetPath does not exist: {asset_path}")
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

    shape_region_ids = set()
    for shape in map_region_shapes:
        region_id = shape.get("regionId")
        if region_id not in region_ids:
            fail(f"map region shape {shape['id']} references missing region {region_id}")
        if region_id in shape_region_ids:
            fail(f"region {region_id} has duplicate map region shapes")
        shape_region_ids.add(region_id)

        boundary = shape.get("boundary", [])
        if len(boundary) < 3:
            fail(f"map region shape {shape['id']} needs at least 3 boundary points")

        center = shape.get("center")
        if not center:
            fail(f"map region shape {shape['id']} missing center")
        assert_point(center, f"map region shape {shape['id']} center")

        label_offset = shape.get("labelOffset")
        if label_offset is None:
            fail(f"map region shape {shape['id']} missing labelOffset")
        assert_point(label_offset, f"map region shape {shape['id']} labelOffset")

        for point_index, point in enumerate(boundary):
            assert_point(point, f"map region shape {shape['id']} boundary[{point_index}]")

        if shape.get("renderOrder", -1) < 0:
            fail(f"map region shape {shape['id']} has negative renderOrder")

        area = abs(polygon_area(boundary))
        if area < 0.05:
            fail(f"map region shape {shape['id']} has too little polygon area: {area:.3f}")

        if not point_in_polygon(center, boundary):
            fail(f"map region shape {shape['id']} center is outside boundary")

        label_point = {
            "x": center["x"] + label_offset["x"],
            "y": center["y"] + label_offset["y"],
        }
        if not point_in_polygon(label_point, boundary):
            fail(f"map region shape {shape['id']} label point is outside boundary")

    missing_shapes = region_ids - shape_region_ids
    if missing_shapes:
        fail(f"missing map region shapes for regions: {sorted(missing_shapes)}")

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

    # 将领验证
    valid_terrains = {"river_plain", "river_delta", "mountain", "mountain_pass", "open_plain",
                      "frontier_plain", "steppe_edge", "huai_river_plain", "mountain_coast", "subtropical"}
    valid_units = {"infantry", "cavalry", "crossbowmen", "siege_engineer", "frontier_cavalry",
                   "garrison", "river_navy", "fire_lance_guard"}

    for general in generals:
        if not general.get("sourceReference"):
            fail(f"general {general['id']} missing sourceReference")
        terrain_bonus = general.get("terrainBonus", {})
        for key in terrain_bonus:
            if key not in valid_terrains:
                fail(f"general {general['id']} has invalid terrainBonus key {key}")
        unit_bonus = general.get("unitBonus", {})
        for key in unit_bonus:
            if key not in valid_units:
                fail(f"general {general['id']} has invalid unitBonus key {key}")

    # 建筑验证
    for building in buildings:
        if not building.get("sourceReference"):
            fail(f"building {building['id']} missing sourceReference")
        requires_tech = building.get("requiresTech", "")
        if requires_tech and requires_tech not in technology_ids:
            fail(f"building {building['id']} requires missing technology {requires_tech}")

    print("DATA VALIDATION PASSED")
    print(
        " ".join(
            [
                f"emperors={len(emperors)}",
                f"portraits={len(portraits)}",
                f"regions={len(regions)}",
                f"map_region_shapes={len(map_region_shapes)}",
                f"historical_layers={len(historical_layers)}",
                f"policies={len(policies)}",
                f"units={len(units)}",
                f"technologies={len(technologies)}",
                f"generals={len(generals)}",
                f"buildings={len(buildings)}",
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


def polygon_area(points):
    area = 0.0
    for i, point in enumerate(points):
        next_point = points[(i + 1) % len(points)]
        area += point.get("x", 0) * next_point.get("y", 0)
        area -= next_point.get("x", 0) * point.get("y", 0)
    return area / 2.0


def assert_point(point, label):
    if not isinstance(point, dict):
        fail(f"{label} must be an object")

    for axis in ["x", "y"]:
        value = point.get(axis)
        if not isinstance(value, (int, float)):
            fail(f"{label} has invalid {axis}: {value}")


def point_in_polygon(point, polygon):
    x = point["x"]
    y = point["y"]
    inside = False
    previous = polygon[-1]

    for current in polygon:
        xi = current["x"]
        yi = current["y"]
        xj = previous["x"]
        yj = previous["y"]

        if point_on_segment(point, previous, current):
            return True

        intersects = (yi > y) != (yj > y)
        if intersects:
            x_at_y = ((xj - xi) * (y - yi) / (yj - yi)) + xi
            if x < x_at_y:
                inside = not inside

        previous = current

    return inside


def point_on_segment(point, start, end):
    px = point["x"]
    py = point["y"]
    ax = start["x"]
    ay = start["y"]
    bx = end["x"]
    by = end["y"]

    cross = (py - ay) * (bx - ax) - (px - ax) * (by - ay)
    if abs(cross) > 1e-6:
        return False

    return (
        min(ax, bx) - 1e-6 <= px <= max(ax, bx) + 1e-6
        and min(ay, by) - 1e-6 <= py <= max(ay, by) + 1e-6
    )


if __name__ == "__main__":
    main()

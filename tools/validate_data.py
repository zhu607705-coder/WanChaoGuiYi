#!/usr/bin/env python3
import json
import re
import struct
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "My project" / "Assets"
DATA = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else ASSETS / "Data"
RESOURCES = ASSETS / "Resources"
UNITY_META_ROOTS = [ASSETS]


def load(name):
    path = DATA / name
    with path.open("r", encoding="utf-8") as f:
        return json.load(f)


def fail(message):
    raise SystemExit(f"DATA VALIDATION FAILED: {message}")


def require_source_reference(item, label):
    item_id = item.get("id", "<missing-id>")
    value = str(item.get("sourceReference", "")).strip()
    if not value:
        fail(f"{label} {item_id} missing sourceReference")

    compact = re.sub(r"\s+", "", value)
    lowered = value.lower()
    placeholder_terms = [
        "todo",
        "missing",
        "placeholder",
        "pending source",
        "source pending",
        "fill later",
        "to be filled",
        "to be supplied",
        "tbd",
    ]
    if set(compact) <= {"?"} or any(term in lowered for term in placeholder_terms):
        fail(f"{label} {item_id} has placeholder sourceReference")

    if len(compact) < 4:
        fail(f"{label} {item_id} sourceReference is too short")


def main():
    emperors = load("emperors.json")["items"]
    portraits = load("portraits.json")["items"]
    regions = load("regions.json")["items"]
    map_region_shape_table = load("map_region_shapes.json")
    map_render_metadata = load("map_render_metadata.json")
    map_region_shapes = map_region_shape_table["items"]
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
        if not (ROOT / "My project" / asset_path).exists():
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
    six_point_shape_count = 0
    if map_region_shape_table.get("precision") == "playable_blockout_v1":
        fail("map region shapes are still using playable_blockout_v1 hex blockout precision")

    for shape in map_region_shapes:
        region_id = shape.get("regionId")
        if region_id not in region_ids:
            fail(f"map region shape {shape['id']} references missing region {region_id}")
        if region_id in shape_region_ids:
            fail(f"region {region_id} has duplicate map region shapes")
        shape_region_ids.add(region_id)

        boundary = shape.get("boundary", [])
        if len(boundary) == 6:
            six_point_shape_count += 1
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
    if six_point_shape_count == len(map_region_shapes):
        fail("map region shapes are still all six-point blockout cells")

    validate_map_render_metadata(map_region_shape_table, map_render_metadata)
    validate_scoped_unity_meta_guids()

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

    validate_strategy_system_defaults(regions, historical_layers)
    validate_heavy_strategy_contract_docs()

    for emperor in emperors:
        required_fields = ["civilization", "legitimacyTypes", "mapScope", "globalMechanicTag"]
        for field in required_fields:
            if not emperor.get(field):
                fail(f"emperor {emperor['id']} missing required global-ready field {field}")

        require_source_reference(emperor, "emperor")

        for policy_id in emperor.get("preferredPolicies", []):
            if policy_id not in policy_ids:
                fail(f"emperor {emperor['id']} references missing policy {policy_id}")

    for policy in policies:
        require_source_reference(policy, "policy")

    for technology in technologies:
        require_source_reference(technology, "technology")

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
        require_source_reference(general, "general")
        portrait_asset_path = general.get("portraitAssetPath", "")
        if not portrait_asset_path.startswith("Assets/Art/Portraits/Generals/") or not portrait_asset_path.endswith(".png"):
            fail(f"general {general['id']} has invalid portraitAssetPath {portrait_asset_path}")
        if not (ROOT / "My project" / portrait_asset_path).exists():
            fail(f"general {general['id']} portraitAssetPath does not exist: {portrait_asset_path}")
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
        require_source_reference(building, "building")
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


def validate_map_render_metadata(shape_table, metadata):
    if metadata.get("precision") != shape_table.get("precision"):
        fail("map render metadata precision does not match map_region_shapes precision")

    shape_center = metadata.get("shapeCenter")
    if not shape_center:
        fail("map render metadata missing shapeCenter")
    assert_point(shape_center, "map render metadata shapeCenter")

    for field in ["pixelsPerShapeUnit", "spritePixelsPerUnit"]:
        value = metadata.get(field)
        if not isinstance(value, (int, float)) or value <= 0:
            fail(f"map render metadata has invalid {field}: {value}")

    source_image = metadata.get("sourceImage", "")
    if source_image != "Assets/Art/Map/jiuzhou_generated_map.png":
        fail(f"map render metadata has invalid sourceImage: {source_image}")

    image_path = ROOT / "My project" / source_image
    if not image_path.exists():
        fail(f"map render source image does not exist: {source_image}")

    width, height = read_png_size(image_path)
    expected = metadata.get("imageSize", {})
    if width != expected.get("width") or height != expected.get("height"):
        fail(f"map render source image size mismatch: got {width}x{height}, expected {expected}")

    resource_image_path = RESOURCES / "Map" / "jiuzhou_generated_map.png"
    if not resource_image_path.exists():
        fail("map render Resources image mirror is missing: Assets/Resources/Map/jiuzhou_generated_map.png")
    if resource_image_path.read_bytes() != image_path.read_bytes():
        fail("map render Resources image mirror differs from Assets/Art/Map/jiuzhou_generated_map.png")

    resource_metadata_path = RESOURCES / "Data" / "map_render_metadata.json"
    if not resource_metadata_path.exists():
        fail("map render Resources metadata mirror is missing: Assets/Resources/Data/map_render_metadata.json")
    with resource_metadata_path.open("r", encoding="utf-8") as handle:
        resource_metadata = json.load(handle)
    if resource_metadata != metadata:
        fail("map render Resources metadata mirror differs from Assets/Data/map_render_metadata.json")

    for meta_path in [
        DATA / "map_render_metadata.json.meta",
        RESOURCES.with_suffix(".meta"),
        RESOURCES / "Map.meta",
        RESOURCES / "Data.meta",
        resource_image_path.with_suffix(resource_image_path.suffix + ".meta"),
        resource_metadata_path.with_suffix(resource_metadata_path.suffix + ".meta"),
    ]:
        assert_unity_meta_guid(meta_path)


def validate_strategy_system_defaults(regions, historical_layers):
    valid_specializations = {
        "grain",
        "military",
        "tax",
        "border",
        "legitimacy",
        "culture",
        "capital",
    }
    layer_by_region = {layer.get("regionId"): layer for layer in historical_layers}
    derived_specializations = set()
    supply_nodes = 0

    for region in regions:
        region_id = region.get("id")
        if not region.get("legitimacyMemory"):
            fail(f"region {region_id} missing legitimacyMemory for governance source gate")
        if not isinstance(region.get("landStructure"), dict):
            fail(f"region {region_id} missing landStructure for governance forecast gate")
        if not region.get("neighbors"):
            fail(f"region {region_id} missing neighbors for connected campaign gate")

        authored_specialization = region.get("regionSpecialization", "")
        if authored_specialization and authored_specialization not in valid_specializations:
            fail(f"region {region_id} has invalid regionSpecialization {authored_specialization}")
        if "supplyNode" in region and not isinstance(region.get("supplyNode"), bool):
            fail(f"region {region_id} supplyNode must be boolean when authored")

        layer = layer_by_region.get(region_id)
        if not layer:
            fail(f"region {region_id} missing historical layer for strategy defaults")
        if not layer.get("uiSummary"):
            fail(f"historical layer for {region_id} missing uiSummary source note")
        if not (layer.get("geographyTags") or layer.get("strategicResources")):
            fail(f"historical layer for {region_id} missing geography/resource tags")

        specialization = derive_strategy_specialization(region, layer)
        derived_specializations.add(specialization)
        if derive_supply_node(region, layer, specialization):
            supply_nodes += 1

    if len(derived_specializations) < 4:
        fail(f"strategy specialization defaults are too narrow: {sorted(derived_specializations)}")
    if supply_nodes < 6:
        fail(f"strategy supply-node defaults are too sparse: {supply_nodes}")


def validate_heavy_strategy_contract_docs():
    contract_path = ROOT / "docs" / "data-contract.md"
    if not contract_path.exists():
        fail("docs/data-contract.md missing for heavy strategy contract gate")

    content = contract_path.read_text(encoding="utf-8")
    required_terms = [
        "regionSpecialization",
        "controlStage",
        "localAcceptance",
        "visibilityState",
        "supplyNode",
        "sourceReference",
        "GovernanceActionForecast",
        "CampaignRouteForecast",
    ]
    for term in required_terms:
        if term not in content:
            fail(f"docs/data-contract.md missing heavy strategy term {term}")


def derive_strategy_specialization(region, layer):
    region_id = region.get("id", "")
    if any(token in region_id for token in ["hexi", "xiyu", "liaodong", "liangzhou", "longxi", "yongzhou"]):
        return "border"
    if any(token in region_id for token in ["guanzhong", "chang_an", "xianyang", "luoyang"]):
        return "capital"
    if any(token in region_id for token in ["jiangnan", "jiangdong", "bashu", "huguang", "shu", "minyue"]):
        return "grain"
    if any(token in region_id for token in ["zhongyuan", "hedong", "hebei", "huainan", "huaibei"]):
        return "tax"
    if any(token in region_id for token in ["qilu", "jingzhou", "wuyue", "lingnan"]):
        return "culture"
    if "military" in region.get("legitimacyMemory", []):
        return "military"
    if region.get("foodOutput", 0) >= region.get("taxOutput", 0):
        return "grain"
    return "tax"


def derive_supply_node(region, layer, specialization):
    if specialization in {"capital", "military", "border"}:
        return True
    joined_tags = " ".join(layer.get("geographyTags", []) + layer.get("strategicResources", []))
    return any(token in joined_tags for token in ["corridor", "pass", "frontier", "horses", "iron", "labor_pool"])


def read_png_size(path):
    with path.open("rb") as handle:
        header = handle.read(24)
    if len(header) < 24 or not header.startswith(b"\x89PNG\r\n\x1a\n"):
        fail(f"map render source image is not a PNG: {path}")
    return struct.unpack(">II", header[16:24])


def assert_unity_meta_guid(path):
    if not path.exists():
        fail(f"missing Unity meta file: {path.relative_to(ROOT / 'My project')}")

    content = path.read_text(encoding="utf-8")
    match = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", content, re.MULTILINE)
    if not match:
        fail(f"Unity meta file has invalid 32-hex guid: {path.relative_to(ROOT / 'My project')}")


def validate_scoped_unity_meta_guids():
    seen_guids = {}

    for root in UNITY_META_ROOTS:
        if not root.exists():
            fail(f"missing Unity asset root: {root.relative_to(ROOT / 'My project')}")

        for meta_path in root.rglob("*.meta"):
            content = meta_path.read_text(encoding="utf-8")
            match = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", content, re.MULTILINE)
            relative_path = meta_path.relative_to(ROOT / "My project")
            if not match:
                fail(f"Unity meta file has invalid 32-hex guid: {relative_path}")

            guid = match.group(1)
            previous_path = seen_guids.get(guid)
            if previous_path:
                fail(f"duplicate Unity meta guid {guid}: {previous_path} and {relative_path}")
            seen_guids[guid] = relative_path


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
import json
import re
import struct
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
WEB_ROOT = ROOT / "web-strategy-map"
SOURCE = WEB_ROOT / "game-data-source"
PUBLIC = WEB_ROOT / "public" / "game-data"
SYNC_SCRIPT = WEB_ROOT / "scripts" / "sync-data.mjs"
PACKAGE_JSON = WEB_ROOT / "package.json"
LOCKED_SCRIPT_PATHS = [
    SYNC_SCRIPT,
    ROOT / "tools" / "art" / "render_jiuzhou_map.py",
    ROOT / "tools" / "art" / "render_jiuzhou_isometric_preview.py",
]

REQUIRED_DATA_JSON = [
    "regions.json",
    "map_region_shapes.json",
    "map_render_metadata.json",
    "historical_layers.json",
    "emperors.json",
    "policies.json",
    "buildings.json",
    "units.json",
    "generals.json",
    "route_networks.json",
    "chronicle_events.json",
    "events.json",
    "portraits.json",
    "talents.json",
    "technologies.json",
    "victory_conditions.json",
]

REQUIRED_AUDIO_JSON = [
    "scene_music.json",
    "emperor_themes.json",
    "chronicle_event_music.json",
    "narration_script.json",
]


def fail(message):
    raise SystemExit(f"WEB DATA SOURCE VALIDATION FAILED: {message}")


def load_json(path):
    try:
        with path.open("r", encoding="utf-8") as handle:
            return json.load(handle)
    except json.JSONDecodeError as exc:
        fail(f"{path.relative_to(ROOT)} is not valid JSON: {exc}")


def require_collection(path):
    payload = load_json(path)
    items = payload.get("items")
    if not isinstance(items, list):
        fail(f"{path.relative_to(ROOT)} must contain an items array")
    return items


def main():
    if not SOURCE.exists():
        fail(f"missing source directory: {SOURCE.relative_to(ROOT)}")

    data_dir = SOURCE / "data"
    audio_dir = SOURCE / "audio"
    map_dir = SOURCE / "map"
    art_dir = SOURCE / "art"
    for path in (data_dir, audio_dir, map_dir, art_dir):
        if not path.exists():
            fail(f"missing source subdirectory: {path.relative_to(ROOT)}")

    collections = {}
    for name in REQUIRED_DATA_JSON:
        path = data_dir / name
        if not path.exists():
            fail(f"missing data file: {path.relative_to(ROOT)}")
        if name != "map_render_metadata.json":
            collections[name] = require_collection(path)
        else:
            metadata = load_json(path)
            source_image = str(metadata.get("sourceImage", ""))
            if source_image.startswith("Assets/") or source_image.startswith("My project/"):
                fail("map_render_metadata.json still points at a Unity asset path")

    for name in REQUIRED_AUDIO_JSON:
        path = audio_dir / name
        if not path.exists():
            fail(f"missing audio data file: {path.relative_to(ROOT)}")
        if name == "narration_script.json":
            load_json(path)
        else:
            require_collection(path)

    map_image = map_dir / "jiuzhou_generated_map.png"
    if not map_image.exists():
        fail(f"missing map image: {map_image.relative_to(ROOT)}")
    if map_image.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n":
        fail("jiuzhou_generated_map.png is not a PNG file")

    emperors = collections["emperors.json"]
    portraits = collections["portraits.json"]
    regions = collections["regions.json"]
    shape_table = load_json(data_dir / "map_region_shapes.json")
    shapes = collections["map_region_shapes.json"]
    map_render_metadata = load_json(data_dir / "map_render_metadata.json")
    historical_layers = collections["historical_layers.json"]
    policies = collections["policies.json"]
    chronicle_events = collections["chronicle_events.json"]
    units = collections["units.json"]
    technologies = collections["technologies.json"]
    generals = collections["generals.json"]
    buildings = collections["buildings.json"]
    route_networks = collections["route_networks.json"]
    if len(regions) < 40:
        fail(f"expected at least 40 regions, got {len(regions)}")
    if len(shapes) != len(regions):
        fail(f"region shape count mismatch: regions={len(regions)} shapes={len(shapes)}")
    if len(chronicle_events) < 200:
        fail(f"expected 200 chronicle events, got {len(chronicle_events)}")

    validate_json_contracts(
        emperors,
        portraits,
        regions,
        shape_table,
        shapes,
        map_render_metadata,
        historical_layers,
        policies,
        chronicle_events,
        units,
        technologies,
        generals,
        buildings,
        route_networks,
        map_dir,
    )

    mp3_files = sorted(SOURCE.glob("audio/**/*.mp3"))
    if not mp3_files:
        fail("no mp3 files found under game-data-source/audio")

    archive_mp3_files = sorted((audio_dir / "archive").glob("**/*.mp3"))
    if len(archive_mp3_files) < 79:
        fail(f"expected at least 79 archived migrated MP3 files, got {len(archive_mp3_files)}")

    art_png_files = sorted(art_dir.glob("**/*.png"))
    if len(art_png_files) < 100:
        fail(f"expected migrated art PNG files under game-data-source/art, got {len(art_png_files)}")
    for path in art_png_files:
        if path.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n":
            fail(f"{path.relative_to(ROOT)} is not a PNG file")

    validate_runtime_icon_assets(art_dir, units)
    validate_art_path_references(collections)

    forbidden_terms = ["My project", "Assets/Data", "Assets/Audio", "Assets/Resources"]
    for script_path in LOCKED_SCRIPT_PATHS:
        script_text = script_path.read_text(encoding="utf-8")
        for term in forbidden_terms:
            if term in script_text:
                fail(f"{script_path.relative_to(ROOT)} still references legacy Unity source term: {term}")

    validate_package_script_locks()

    if PUBLIC.exists():
        compare_synced_tree(SOURCE, PUBLIC)

    print(
        "WEB DATA SOURCE VALIDATION PASSED "
        f"data={len(REQUIRED_DATA_JSON)} audioJson={len(REQUIRED_AUDIO_JSON)} "
        f"regions={len(regions)} chronicleEvents={len(chronicle_events)} "
        f"mp3={len(mp3_files)} archiveMp3={len(archive_mp3_files)} artPng={len(art_png_files)}"
    )


def compare_synced_tree(source_root, public_root):
    source_files = {
        path.relative_to(source_root).as_posix()
        for path in source_root.rglob("*")
        if path.is_file() and path.name != "README.md"
    }
    public_files = {
        path.relative_to(public_root).as_posix()
        for path in public_root.rglob("*")
        if path.is_file()
    }
    missing = sorted(source_files - public_files)
    extra = sorted(public_files - source_files)
    if missing:
        fail(f"public/game-data is missing synced files: {missing[:5]}")
    if extra:
        fail(f"public/game-data has stale files: {extra[:5]}")


def validate_json_contracts(
    emperors,
    portraits,
    regions,
    shape_table,
    shapes,
    map_render_metadata,
    historical_layers,
    policies,
    chronicle_events,
    units,
    technologies,
    generals,
    buildings,
    route_networks,
    map_dir,
):
    if len(emperors) < 8:
        fail(f"MVP requires at least 8 emperors, got {len(emperors)}")

    emperor_ids = assert_unique_ids(emperors, "emperor")
    assert_unique_ids(portraits, "portrait")
    region_ids = assert_unique_ids(regions, "region")
    assert_unique_ids(shapes, "map region shape")
    assert_unique_ids(historical_layers, "historical layer")
    policy_ids = assert_unique_ids(policies, "policy")
    chronicle_event_ids = assert_unique_ids(chronicle_events, "chronicle event")
    unit_ids = assert_unique_ids(units, "unit")
    technology_ids = assert_unique_ids(technologies, "technology")
    general_ids = assert_unique_ids(generals, "general")
    assert_unique_ids(buildings, "building")
    assert_unique_ids(route_networks, "route network")

    validate_portraits(portraits, emperor_ids)
    validate_regions_and_shapes(regions, shapes, shape_table, map_render_metadata, map_dir)
    validate_route_networks(route_networks, {region["id"]: region for region in regions})
    validate_historical_layers(historical_layers, region_ids, technology_ids)
    validate_strategy_system_defaults(regions, historical_layers)
    validate_heavy_strategy_contract_docs()

    for emperor in emperors:
        for field in ["civilization", "legitimacyTypes", "mapScope", "globalMechanicTag"]:
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

    validate_generals(generals, unit_ids)
    validate_buildings(buildings, technology_ids)


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


def validate_portraits(portraits, emperor_ids):
    if not portraits:
        fail("portrait table is empty")

    portrait_emperor_ids = set()
    for portrait in portraits:
        emperor_id = portrait.get("emperorId")
        if emperor_id not in emperor_ids:
            fail(f"portrait {portrait['id']} references missing emperor {emperor_id}")
        portrait_emperor_ids.add(emperor_id)
        if not portrait.get("prompt"):
            fail(f"portrait {portrait['id']} missing generation prompt")

    missing_portraits = emperor_ids - portrait_emperor_ids
    if missing_portraits:
        print(f"WARNING: missing portraits for emperors: {sorted(missing_portraits)}")


def validate_regions_and_shapes(regions, shapes, shape_table, metadata, map_dir):
    region_ids = {region["id"] for region in regions}
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

    if shape_table.get("precision") == "playable_blockout_v1":
        fail("map region shapes are still using playable_blockout_v1 hex blockout precision")

    shape_region_ids = set()
    six_point_shape_count = 0
    for shape in shapes:
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
    if six_point_shape_count == len(shapes):
        fail("map region shapes are still all six-point blockout cells")

    validate_map_render_metadata(shape_table, metadata, map_dir)


def validate_historical_layers(historical_layers, region_ids, technology_ids):
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


def validate_map_render_metadata(shape_table, metadata, map_dir):
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
    if source_image != "/game-data/map/jiuzhou_generated_map.png":
        fail(f"map render metadata has invalid sourceImage: {source_image}")

    image_path = map_dir / "jiuzhou_generated_map.png"
    width, height = read_png_size(image_path)
    expected = metadata.get("imageSize", {})
    if width != expected.get("width") or height != expected.get("height"):
        fail(f"map render source image size mismatch: got {width}x{height}, expected {expected}")


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


def validate_route_networks(route_networks, region_by_id):
    valid_road_classes = {
        "open-road",
        "river-road",
        "hill-road",
        "pass-bottleneck",
        "frontier-track",
        "water-network",
    }
    blockade_fields = [
        "initialStrengthFloor",
        "refreshStrengthGain",
        "guardFoodCost",
        "guardMoneyCost",
        "guardStrengthGain",
        "guardBlockadeReduction",
        "guardRiskReduction",
        "guardDamageReduction",
        "clearFoodCost",
        "clearMoneyCost",
        "clearGuardStrengthGain",
        "clearRiskReduction",
    ]

    if not route_networks:
        fail("route_networks.json has no route network items")

    for network in route_networks:
        network_id = network.get("id", "")
        if not re.fullmatch(r"[a-z0-9_]+", network_id):
            fail(f"route network {network_id or '<missing-id>'} id must be snake_case")
        if not str(network.get("label", "")).strip():
            fail(f"route network {network_id} missing label")
        require_source_reference(network, "route network")

        nodes = network.get("nodes", [])
        if not isinstance(nodes, list) or len(nodes) < 2:
            fail(f"route network {network_id} needs at least two nodes")
        if len(nodes) != len(set(nodes)):
            fail(f"route network {network_id} has duplicate nodes")
        for node_id in nodes:
            if node_id not in region_by_id:
                fail(f"route network {network_id} references missing region {node_id}")

        for from_id, to_id in zip(nodes, nodes[1:]):
            from_neighbors = set(region_by_id[from_id].get("neighbors", []))
            to_neighbors = set(region_by_id[to_id].get("neighbors", []))
            if to_id not in from_neighbors or from_id not in to_neighbors:
                fail(f"route network {network_id} leg {from_id}->{to_id} is not bidirectional region adjacency")

        road_class = network.get("roadClass")
        if road_class not in valid_road_classes:
            fail(f"route network {network_id} has invalid roadClass {road_class}")

        base_capacity = network.get("baseCapacity")
        if not isinstance(base_capacity, (int, float)) or base_capacity <= 0:
            fail(f"route network {network_id} has invalid baseCapacity {base_capacity}")
        supply_factor = network.get("supplyFactor")
        if not isinstance(supply_factor, (int, float)) or supply_factor <= 0:
            fail(f"route network {network_id} has invalid supplyFactor {supply_factor}")
        interception_modifier = network.get("interceptionModifier")
        if not isinstance(interception_modifier, (int, float)):
            fail(f"route network {network_id} has invalid interceptionModifier {interception_modifier}")
        if not str(network.get("reason", "")).strip():
            fail(f"route network {network_id} missing reason")

        blockade = network.get("blockade")
        if not isinstance(blockade, dict):
            fail(f"route network {network_id} missing blockade object")
        for field in blockade_fields:
            value = blockade.get(field)
            if not isinstance(value, (int, float)) or value < 0:
                fail(f"route network {network_id} blockade.{field} must be a non-negative number")


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


def validate_generals(generals, unit_ids):
    valid_terrains = {
        "river_plain",
        "river_delta",
        "mountain",
        "mountain_pass",
        "open_plain",
        "frontier_plain",
        "steppe_edge",
        "huai_river_plain",
        "mountain_coast",
        "subtropical",
    }
    for general in generals:
        require_source_reference(general, "general")
        for key in general.get("terrainBonus", {}):
            if key not in valid_terrains:
                fail(f"general {general['id']} has invalid terrainBonus key {key}")
        for key in general.get("unitBonus", {}):
            if key not in unit_ids:
                fail(f"general {general['id']} has invalid unitBonus key {key}")


def validate_buildings(buildings, technology_ids):
    for building in buildings:
        require_source_reference(building, "building")
        requires_tech = building.get("requiresTech", "")
        if requires_tech and requires_tech not in technology_ids:
            fail(f"building {building['id']} requires missing technology {requires_tech}")


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
        fail(f"map render source image is not a PNG: {path.relative_to(ROOT)}")
    return struct.unpack(">II", header[16:24])


def validate_package_script_locks():
    package = load_json(PACKAGE_JSON)
    scripts = package.get("scripts")
    if not isinstance(scripts, dict):
        fail("package.json must contain scripts")

    for script_name in ["dev", "build", "test:ui"]:
        script = scripts.get(script_name, "")
        if "npm run sync:data" not in script:
            fail(f"package script {script_name} must run sync:data")
        if "npm run check:data-source" not in script:
            fail(f"package script {script_name} must run check:data-source")

    check_script = scripts.get("check:data-source", "")
    if "validate_web_data_source.py" not in check_script:
        fail("package script check:data-source must call validate_web_data_source.py")


def validate_runtime_icon_assets(art_dir, units):
    required_system_icons = [
        "food",
        "money",
        "population",
        "manpower",
        "legitimacy",
        "rebellion_risk",
        "land_annexation",
        "fiscal_collapse",
        "succession_crisis",
        "local_separatism",
    ]
    for icon_name in required_system_icons:
        path = art_dir / "Icons" / "Systems" / f"{icon_name}.png"
        if not path.exists():
            fail(f"missing runtime system icon: {path.relative_to(ROOT)}")

    for unit in units:
        unit_id = str(unit.get("id", ""))
        path = art_dir / "Icons" / "Units" / f"{unit_id}.png"
        if not path.exists():
            fail(f"missing runtime unit icon for {unit_id}: {path.relative_to(ROOT)}")


def validate_art_path_references(collections):
    for portrait in collections["portraits.json"]:
        asset_path = str(portrait.get("assetPath", ""))
        if not asset_path.startswith("art/Portraits/") or not asset_path.endswith(".png"):
            fail(f"portrait {portrait.get('id')} has invalid assetPath: {asset_path}")
        if not (SOURCE / asset_path).exists():
            fail(f"portrait {portrait.get('id')} assetPath does not exist: {asset_path}")

    for general in collections["generals.json"]:
        asset_path = str(general.get("portraitAssetPath", ""))
        if not asset_path.startswith("art/Portraits/Generals/") or not asset_path.endswith(".png"):
            fail(f"general {general.get('id')} has invalid portraitAssetPath: {asset_path}")
        if not (SOURCE / asset_path).exists():
            fail(f"general {general.get('id')} portraitAssetPath does not exist: {asset_path}")


if __name__ == "__main__":
    try:
        main()
    except BrokenPipeError:
        sys.exit(1)

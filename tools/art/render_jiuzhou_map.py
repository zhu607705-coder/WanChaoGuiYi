#!/usr/bin/env python3
import argparse
import json
import math
import random
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = ROOT / "My project" / "Assets" / "Data"
MAP_PATH = ROOT / "My project" / "Assets" / "Art" / "Map" / "jiuzhou_generated_map.png"
METADATA_PATH = DATA_DIR / "map_render_metadata.json"
RESOURCE_MAP_PATH = ROOT / "My project" / "Assets" / "Resources" / "Map" / "jiuzhou_generated_map.png"
RESOURCE_METADATA_PATH = ROOT / "My project" / "Assets" / "Resources" / "Data" / "map_render_metadata.json"

WIDTH = 2048
HEIGHT = 1536
SHAPE_CENTER = (-3.65, 0.05)
PIXELS_PER_SHAPE_UNIT = 86.0
SPRITE_PIXELS_PER_UNIT = 100.0
MAP_PRECISION = "map_aligned_region_v1"

HULL = [
    (-13.9, 5.35),
    (-12.3, 6.35),
    (-8.2, 7.05),
    (-4.5, 7.15),
    (-0.8, 6.85),
    (2.6, 6.65),
    (5.8, 5.85),
    (6.35, 4.6),
    (5.9, 2.3),
    (4.7, 0.2),
    (5.25, -2.2),
    (4.1, -3.7),
    (2.6, -4.5),
    (2.0, -5.8),
    (0.6, -6.75),
    (-1.9, -6.95),
    (-4.7, -6.55),
    (-6.8, -6.2),
    (-7.9, -4.9),
    (-7.35, -3.05),
    (-8.15, -1.25),
    (-7.55, 0.55),
    (-8.65, 2.15),
    (-10.7, 3.65),
]

TERRAIN_COLORS = {
    "plain": (139, 124, 72),
    "open_plain": (139, 124, 72),
    "river_plain": (98, 132, 111),
    "river_delta": (82, 130, 128),
    "basin": (116, 126, 74),
    "hill": (132, 94, 80),
    "mountain": (92, 112, 78),
    "mountain_pass": (92, 112, 78),
    "mountain_plateau": (103, 91, 125),
    "mountain_coast": (77, 121, 113),
    "frontier_plain": (111, 93, 142),
    "frontier_forest": (76, 116, 105),
    "plateau": (119, 94, 55),
    "corridor": (137, 101, 62),
    "desert_oasis": (102, 88, 135),
    "subtropical": (114, 126, 66),
}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--refine-shapes", action="store_true")
    parser.add_argument("--render-map", action="store_true")
    args = parser.parse_args()

    if not args.refine_shapes and not args.render_map:
        args.refine_shapes = True
        args.render_map = True

    regions = load_json(DATA_DIR / "regions.json")
    shapes = load_json(DATA_DIR / "map_region_shapes.json")

    if args.refine_shapes:
        refine_shapes(regions, shapes)
        save_json(DATA_DIR / "map_region_shapes.json", shapes)

    if args.render_map:
        render_map(regions, shapes, MAP_PATH)
        write_metadata(METADATA_PATH, shapes)
        sync_resource_mirror()

    print(
        "Rendered Jiuzhou map:",
        f"precision={shapes['precision']}",
        f"regions={len(shapes['items'])}",
        f"image={MAP_PATH}",
    )


def load_json(path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def save_json(path, payload):
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(payload, handle, ensure_ascii=False, indent=2)
        handle.write("\n")


def refine_shapes(region_table, shape_table):
    region_by_id = {item["id"]: item for item in region_table["items"]}
    original_shapes = shape_table["items"]
    centers = [(shape["regionId"], (shape["center"]["x"], shape["center"]["y"])) for shape in original_shapes]

    refined = []
    for shape in original_shapes:
        region_id = shape["regionId"]
        center = (shape["center"]["x"], shape["center"]["y"])
        polygon = build_voronoi_cell(region_id, center, centers)
        if abs(polygon_area(polygon)) < 0.08:
            polygon = build_irregular_local_cell(region_id, center, shape.get("renderOrder", 10))

        if polygon_area(polygon) < 0:
            polygon = list(reversed(polygon))

        refined.append(
            {
                "id": shape["id"],
                "regionId": region_id,
                "center": round_point(center),
                "labelOffset": {"x": 0.0, "y": 0.0},
                "renderOrder": shape.get("renderOrder", 10),
                "boundary": [round_point(point) for point in simplify_polygon(polygon)],
            }
        )

    shape_table["precision"] = MAP_PRECISION
    shape_table[
        "designReference"
    ] = "Map-aligned region polygons generated from stable region centers; used by both the PNG map source and Unity interaction meshes."
    shape_table["items"] = refined


def build_voronoi_cell(region_id, center, centers):
    polygon = list(HULL)
    cx, cy = center
    for other_id, other in centers:
        if other_id == region_id:
            continue
        ox, oy = other
        midpoint = ((cx + ox) * 0.5, (cy + oy) * 0.5)
        normal = (ox - cx, oy - cy)
        polygon = clip_polygon_to_half_plane(polygon, midpoint, normal)
        if len(polygon) < 3:
            return build_irregular_local_cell(region_id, center, 10)

    return polygon


def clip_polygon_to_half_plane(polygon, midpoint, normal):
    output = []
    if not polygon:
        return output

    previous = polygon[-1]
    previous_inside = is_inside_half_plane(previous, midpoint, normal)
    for current in polygon:
        current_inside = is_inside_half_plane(current, midpoint, normal)
        if current_inside != previous_inside:
            output.append(line_intersection(previous, current, midpoint, normal))
        if current_inside:
            output.append(current)
        previous = current
        previous_inside = current_inside

    return output


def is_inside_half_plane(point, midpoint, normal):
    return ((point[0] - midpoint[0]) * normal[0] + (point[1] - midpoint[1]) * normal[1]) <= 1e-9


def line_intersection(start, end, midpoint, normal):
    sx, sy = start
    ex, ey = end
    start_value = (sx - midpoint[0]) * normal[0] + (sy - midpoint[1]) * normal[1]
    end_value = (ex - midpoint[0]) * normal[0] + (ey - midpoint[1]) * normal[1]
    denominator = start_value - end_value
    if abs(denominator) < 1e-9:
        return end

    t = start_value / denominator
    return (sx + (ex - sx) * t, sy + (ey - sy) * t)


def build_irregular_local_cell(region_id, center, render_order):
    rng = random.Random(region_id)
    point_count = 8 if render_order >= 20 else 10
    radius = 0.32 if render_order >= 20 else 0.55
    points = []
    for index in range(point_count):
        angle = (math.tau * index / point_count) + rng.uniform(-0.08, 0.08)
        local_radius = radius * rng.uniform(0.82, 1.18)
        points.append((center[0] + math.cos(angle) * local_radius, center[1] + math.sin(angle) * local_radius))
    return points


def simplify_polygon(polygon):
    rounded = []
    for point in polygon:
        candidate = (round(point[0], 3), round(point[1], 3))
        if rounded and distance(rounded[-1], candidate) < 0.025:
            continue
        rounded.append(candidate)

    if len(rounded) > 2 and distance(rounded[0], rounded[-1]) < 0.025:
        rounded.pop()

    return rounded


def distance(a, b):
    return math.hypot(a[0] - b[0], a[1] - b[1])


def polygon_area(points):
    area = 0.0
    for index, point in enumerate(points):
        next_point = points[(index + 1) % len(points)]
        area += point[0] * next_point[1] - next_point[0] * point[1]
    return area * 0.5


def round_point(point):
    return {"x": round(point[0], 3), "y": round(point[1], 3)}


def render_map(region_table, shape_table, path):
    regions = {item["id"]: item for item in region_table["items"]}
    image = Image.new("RGB", (WIDTH, HEIGHT), (34, 55, 60))
    draw = ImageDraw.Draw(image)

    draw.rectangle([(0, 0), (WIDTH, HEIGHT)], fill=(39, 62, 67))
    draw.rectangle([(0, 0), (WIDTH, 120)], fill=(28, 72, 88))
    draw.rectangle([(0, HEIGHT - 150), (WIDTH, HEIGHT)], fill=(31, 80, 89))

    hull_pixels = [to_pixel(point) for point in HULL]
    draw.polygon(hull_pixels, fill=(52, 46, 35), outline=(28, 24, 19))

    for shape in sorted(shape_table["items"], key=lambda item: item.get("renderOrder", 10)):
        region = regions.get(shape["regionId"], {})
        terrain = region.get("terrain", "plain")
        fill = TERRAIN_COLORS.get(terrain, (124, 112, 76))
        polygon = [to_pixel((point["x"], point["y"])) for point in shape["boundary"]]
        draw.polygon(polygon, fill=fill, outline=(24, 22, 18))

    draw_river(draw, [(-7.7, 4.0), (-6.2, 2.8), (-5.0, 1.7), (-3.0, 1.15), (-1.1, 0.45), (0.8, -0.2), (2.7, -1.0), (4.8, -1.9)])
    draw_river(draw, [(-7.0, -1.2), (-5.4, -2.0), (-3.0, -2.9), (-0.5, -3.35), (1.6, -3.05), (3.5, -2.7), (5.1, -2.55)])

    title_font = load_font(64)
    label_font = load_font(34)
    city_font = load_font(28)
    draw_centered_text(draw, (WIDTH // 2, 64), "\u4e07\u671d\u5f52\u4e00\u30fb\u4e5d\u5dde\u6218\u7565\u56fe", title_font, (235, 217, 165))

    for shape in sorted(shape_table["items"], key=lambda item: item.get("renderOrder", 10)):
        region = regions.get(shape["regionId"], {})
        name = region.get("name", shape["regionId"])
        center = shape["center"]
        font = city_font if shape.get("renderOrder", 10) >= 20 else label_font
        draw_label(draw, to_pixel((center["x"], center["y"])), name, font)

    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def write_metadata(path, shape_table):
    metadata = {
        "schemaVersion": 1,
        "mapScope": "china",
        "precision": shape_table["precision"],
        "sourceImage": "Assets/Art/Map/jiuzhou_generated_map.png",
        "imageSize": {
            "width": WIDTH,
            "height": HEIGHT,
        },
        "shapeCenter": {
            "x": SHAPE_CENTER[0],
            "y": SHAPE_CENTER[1],
        },
        "pixelsPerShapeUnit": PIXELS_PER_SHAPE_UNIT,
        "spritePixelsPerUnit": SPRITE_PIXELS_PER_UNIT,
    }
    save_json(path, metadata)


def sync_resource_mirror():
    RESOURCE_MAP_PATH.parent.mkdir(parents=True, exist_ok=True)
    RESOURCE_METADATA_PATH.parent.mkdir(parents=True, exist_ok=True)
    RESOURCE_MAP_PATH.write_bytes(MAP_PATH.read_bytes())
    RESOURCE_METADATA_PATH.write_text(METADATA_PATH.read_text(encoding="utf-8"), encoding="utf-8", newline="\n")


def draw_river(draw, points):
    pixels = [to_pixel(point) for point in points]
    draw.line(pixels, fill=(74, 143, 175), width=8, joint="curve")
    draw.line(pixels, fill=(105, 173, 198), width=3, joint="curve")


def draw_label(draw, center, text, font):
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    padding_x = 12
    padding_y = 7
    x = center[0] - text_width // 2
    y = center[1] - text_height // 2
    rect = [x - padding_x, y - padding_y, x + text_width + padding_x, y + text_height + padding_y]
    draw.rounded_rectangle(rect, radius=10, fill=(12, 11, 8), outline=(64, 55, 36))
    draw.text((x, y), text, font=font, fill=(238, 225, 177))


def draw_centered_text(draw, center, text, font, fill):
    bbox = draw.textbbox((0, 0), text, font=font)
    draw.text((center[0] - (bbox[2] - bbox[0]) // 2, center[1] - (bbox[3] - bbox[1]) // 2), text, font=font, fill=fill)


def load_font(size):
    candidates = [
        Path("C:/Windows/Fonts/msyh.ttc"),
        Path("C:/Windows/Fonts/simhei.ttf"),
        Path("C:/Windows/Fonts/simsun.ttc"),
    ]
    for candidate in candidates:
        if candidate.exists():
            return ImageFont.truetype(str(candidate), size=size)
    return ImageFont.load_default()


def to_pixel(point):
    x = int(round(WIDTH / 2 + (point[0] - SHAPE_CENTER[0]) * PIXELS_PER_SHAPE_UNIT))
    y = int(round(HEIGHT / 2 - (point[1] - SHAPE_CENTER[1]) * PIXELS_PER_SHAPE_UNIT))
    return (x, y)


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
import argparse
import json
import math
from collections import deque
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = ROOT / "My project" / "Assets" / "Data"
DEFAULT_OUTPUT = ROOT / ".outputs" / "visual" / "jiuzhou_isometric_preview.png"

WIDTH = 1920
HEIGHT = 1280
MARGIN_X = 130
MARGIN_Y = 150

TERRAIN_COLORS = {
    "plain": (139, 124, 72),
    "open_plain": (139, 124, 72),
    "river_plain": (82, 127, 116),
    "river_delta": (74, 128, 132),
    "basin": (112, 130, 78),
    "hill": (132, 98, 76),
    "mountain": (92, 112, 83),
    "mountain_pass": (96, 108, 88),
    "mountain_plateau": (112, 94, 130),
    "mountain_coast": (78, 120, 112),
    "frontier_plain": (112, 96, 142),
    "frontier_forest": (78, 116, 106),
    "plateau": (126, 98, 58),
    "corridor": (138, 104, 66),
    "desert_oasis": (116, 94, 136),
    "subtropical": (114, 132, 72),
}

TERRAIN_HEIGHTS = {
    "river_delta": 16,
    "river_plain": 18,
    "plain": 24,
    "open_plain": 24,
    "subtropical": 26,
    "basin": 30,
    "corridor": 34,
    "frontier_plain": 36,
    "frontier_forest": 40,
    "hill": 46,
    "desert_oasis": 46,
    "plateau": 54,
    "mountain_coast": 56,
    "mountain_pass": 60,
    "mountain_plateau": 64,
    "mountain": 72,
}

ROUTES = [
    ("guanzhong", "zhongyuan"),
    ("bashu", "jiangdong"),
    ("liaodong", "guanzhong"),
]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", default=str(DEFAULT_OUTPUT))
    parser.add_argument("--width", type=int, default=WIDTH)
    parser.add_argument("--height", type=int, default=HEIGHT)
    parser.add_argument("--no-routes", action="store_true")
    args = parser.parse_args()

    output_path = Path(args.output)
    regions = load_json(DATA_DIR / "regions.json")["items"]
    shapes = load_json(DATA_DIR / "map_region_shapes.json")["items"]

    renderer = IsometricPreviewRenderer(regions, shapes, args.width, args.height)
    image = renderer.render(show_routes=not args.no_routes)
    output_path.parent.mkdir(parents=True, exist_ok=True)
    image.save(output_path)

    stats = renderer.stats(output_path)
    print(json.dumps(stats, ensure_ascii=False, indent=2))


def load_json(path):
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


class IsometricPreviewRenderer:
    def __init__(self, regions, shapes, width, height):
        self.regions = {region["id"]: region for region in regions}
        self.shapes = shapes
        self.width = width
        self.height = height
        self.font_title = load_font(50)
        self.font_label = load_font(22)
        self.font_small = load_font(18)
        self.font_tiny = load_font(15)
        self.region_height = {}
        self.bounds = self.compute_bounds()

    def render(self, show_routes=True):
        image = Image.new("RGBA", (self.width, self.height), (31, 56, 64, 255))
        draw = ImageDraw.Draw(image, "RGBA")
        self.draw_background(draw)

        draw_order = sorted(self.shapes, key=lambda shape: self.project_point(shape["center"])[1])
        for shape in draw_order:
            self.draw_region(draw, shape)

        if show_routes:
            self.draw_route_overlay(draw)

        self.draw_title(draw)
        self.draw_legend(draw)
        return image.convert("RGB")

    def compute_bounds(self):
        raw_points = []
        for shape in self.shapes:
            region = self.regions.get(shape["regionId"], {})
            terrain = region.get("terrain", "plain")
            height = self.compute_region_height(region, terrain)
            self.region_height[shape["regionId"]] = height
            for point in shape["boundary"]:
                x, y = self.raw_project(point)
                raw_points.append((x, y))
                raw_points.append((x, y - height))

        min_x = min(point[0] for point in raw_points)
        max_x = max(point[0] for point in raw_points)
        min_y = min(point[1] for point in raw_points)
        max_y = max(point[1] for point in raw_points)
        scale_x = (self.width - MARGIN_X * 2) / max(1.0, max_x - min_x)
        scale_y = (self.height - MARGIN_Y * 2) / max(1.0, max_y - min_y)
        scale = min(scale_x, scale_y)
        offset_x = (self.width - (max_x - min_x) * scale) * 0.5 - min_x * scale
        offset_y = (self.height - (max_y - min_y) * scale) * 0.54 - min_y * scale
        return {
            "scale": scale,
            "offset_x": offset_x,
            "offset_y": offset_y,
        }

    def compute_region_height(self, region, terrain):
        base = TERRAIN_HEIGHTS.get(terrain, 28)
        population = max(1, int(region.get("population", 1)))
        population_bonus = max(0.0, math.log10(population) - 5.4) * 7.0
        local_power_bonus = float(region.get("localPower", 0)) * 0.08
        rebellion_bonus = float(region.get("rebellionRisk", 0)) * 0.04
        return base + population_bonus + local_power_bonus + rebellion_bonus

    def raw_project(self, point):
        x = float(point["x"])
        y = float(point["y"])
        return (x * 86.0, -y * 58.0)

    def project_point(self, point, lift=0.0):
        x, y = self.raw_project(point)
        scale = self.bounds["scale"]
        return (
            int(round(x * scale + self.bounds["offset_x"])),
            int(round((y - lift) * scale + self.bounds["offset_y"])),
        )

    def draw_background(self, draw):
        draw.rectangle((0, 0, self.width, self.height), fill=(30, 58, 66, 255))
        for i in range(14):
            alpha = 38 - i * 2
            y = 80 + i * 70
            draw.line((80, y, self.width - 80, y + 32), fill=(84, 125, 130, alpha), width=2)

        draw.ellipse(
            (self.width * 0.18, self.height * 0.18, self.width * 0.88, self.height * 0.98),
            fill=(20, 35, 39, 70),
        )

    def draw_region(self, draw, shape):
        region_id = shape["regionId"]
        region = self.regions.get(region_id, {})
        terrain = region.get("terrain", "plain")
        base_color = TERRAIN_COLORS.get(terrain, (124, 112, 76))
        lift = self.region_height[region_id]
        bottom = [self.project_point(point) for point in shape["boundary"]]
        top = [self.project_point(point, lift=lift) for point in shape["boundary"]]

        shadow = [(x + 12, y + 18) for x, y in bottom]
        draw.polygon(shadow, fill=(6, 10, 11, 62))

        side_faces = []
        for index in range(len(top)):
            next_index = (index + 1) % len(top)
            face = [top[index], top[next_index], bottom[next_index], bottom[index]]
            face_depth = (bottom[index][1] + bottom[next_index][1]) * 0.5
            edge_dx = bottom[next_index][0] - bottom[index][0]
            shade = 0.43 + min(0.18, abs(edge_dx) / 900.0)
            side_faces.append((face_depth, face, shade))

        for _, face, shade in sorted(side_faces, key=lambda item: item[0]):
            draw.polygon(face, fill=darken(base_color, shade) + (255,), outline=(32, 27, 20, 180))

        risk = float(region.get("rebellionRisk", 0))
        outline = (224, 116, 70, 210) if risk >= 18 else (28, 25, 18, 220)
        draw.polygon(top, fill=lighten(base_color, 1.12) + (255,), outline=outline)

        self.draw_region_label(draw, shape, region, lift)

    def draw_region_label(self, draw, shape, region, lift):
        center = self.project_point(shape["center"], lift=lift + 6)
        text = region.get("name", shape["regionId"])
        bbox = draw.textbbox((0, 0), text, font=self.font_tiny)
        text_width = bbox[2] - bbox[0]
        text_height = bbox[3] - bbox[1]
        rect = (
            center[0] - text_width // 2 - 7,
            center[1] - text_height // 2 - 4,
            center[0] + text_width // 2 + 7,
            center[1] + text_height // 2 + 5,
        )
        draw.rounded_rectangle(rect, radius=5, fill=(11, 10, 7, 178), outline=(221, 198, 126, 78))
        draw.text((center[0] - text_width // 2, center[1] - text_height // 2), text, font=self.font_tiny, fill=(242, 229, 174, 235))

    def draw_route_overlay(self, draw):
        graph = {region["id"]: region.get("neighbors", []) for region in self.regions.values()}
        colors = [(234, 210, 95, 230), (97, 202, 221, 230), (232, 114, 92, 230)]
        for index, (start, end) in enumerate(ROUTES):
            route = find_route(graph, start, end)
            if len(route) < 2:
                continue

            points = [self.route_point(region_id) for region_id in route]
            color = colors[index % len(colors)]
            draw.line(points, fill=(8, 9, 8, 180), width=9, joint="curve")
            draw.line(points, fill=color, width=4, joint="curve")
            for point in points:
                draw.ellipse((point[0] - 6, point[1] - 6, point[0] + 6, point[1] + 6), fill=color)

            self.draw_arrow_head(draw, points[-2], points[-1], color)

    def route_point(self, region_id):
        shape = next(shape for shape in self.shapes if shape["regionId"] == region_id)
        lift = self.region_height.get(region_id, 28) + 18
        return self.project_point(shape["center"], lift=lift)

    def draw_arrow_head(self, draw, start, end, color):
        dx = end[0] - start[0]
        dy = end[1] - start[1]
        length = math.hypot(dx, dy)
        if length <= 0:
            return
        ux = dx / length
        uy = dy / length
        px = -uy
        py = ux
        tip = end
        left = (int(end[0] - ux * 22 + px * 10), int(end[1] - uy * 22 + py * 10))
        right = (int(end[0] - ux * 22 - px * 10), int(end[1] - uy * 22 - py * 10))
        draw.polygon([tip, left, right], fill=color)

    def draw_title(self, draw):
        title = "万朝归一 九州 2.5D 地形预览"
        subtitle = "区域边界来自 map_region_shapes.json；高度由 terrain/population/localPower/rebellionRisk 驱动"
        draw.text((74, 54), title, font=self.font_title, fill=(245, 224, 166, 255))
        draw.text((78, 116), subtitle, font=self.font_small, fill=(204, 222, 211, 230))

    def draw_legend(self, draw):
        x = self.width - 430
        y = 62
        draw.rounded_rectangle((x - 22, y - 18, self.width - 58, y + 202), radius=8, fill=(9, 13, 12, 178), outline=(190, 166, 98, 90))
        draw.text((x, y), "视觉规则", font=self.font_label, fill=(244, 225, 168, 255))
        lines = [
            "地形颜色：regions.json terrain",
            "立体高度：地形 + 人口 + 地方势力",
            "橙色边线：较高民变风险",
            "行军线：由真实 neighbors 图搜索",
            "用途：编辑器外快速验证 3D 方向",
        ]
        for index, line in enumerate(lines):
            draw.text((x, y + 38 + index * 29), line, font=self.font_small, fill=(219, 231, 219, 235))

    def stats(self, output_path):
        return {
            "output": str(output_path),
            "regions": len(self.shapes),
            "width": self.width,
            "height": self.height,
            "heightRange": {
                "min": round(min(self.region_height.values()), 2),
                "max": round(max(self.region_height.values()), 2),
            },
            "routes": [
                {"from": start, "to": end}
                for start, end in ROUTES
            ],
        }


def find_route(graph, start, end):
    if start not in graph or end not in graph:
        return []

    queue = deque([start])
    previous = {start: None}
    while queue:
        current = queue.popleft()
        if current == end:
            break
        for neighbor in graph.get(current, []):
            if neighbor in previous:
                continue
            previous[neighbor] = current
            queue.append(neighbor)

    if end not in previous:
        return []

    route = []
    current = end
    while current is not None:
        route.append(current)
        current = previous[current]
    return list(reversed(route))


def darken(color, factor):
    return tuple(max(0, min(255, int(channel * factor))) for channel in color)


def lighten(color, factor):
    return tuple(max(0, min(255, int(channel * factor))) for channel in color)


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


if __name__ == "__main__":
    main()

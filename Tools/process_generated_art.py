"""Normalize generated pixel-art atlases into small Godot-ready PNG assets."""

from __future__ import annotations

import argparse
from collections import deque
from pathlib import Path

from PIL import Image


UNIT_NAMES = ("fighter", "scout", "mage")
FACING_NAMES = ("north", "east", "south", "west")
TERRAIN_NAMES = ("grass", "dirt", "rock")


def cell_bounds(length: int, index: int, count: int) -> tuple[int, int]:
    return round(length * index / count), round(length * (index + 1) / count)


def is_checker_pixel(pixel: tuple[int, int, int, int]) -> bool:
    red, green, blue, _ = pixel
    return min(red, green, blue) >= 220 and max(red, green, blue) - min(red, green, blue) <= 16


def clear_connected_checkerboard(image: Image.Image) -> Image.Image:
    """Remove only the light neutral checkerboard connected to the cell edges."""
    rgba = image.convert("RGBA")
    width, height = rgba.size
    pixels = rgba.load()
    queue: deque[tuple[int, int]] = deque()
    seen: set[tuple[int, int]] = set()

    for x in range(width):
        queue.append((x, 0))
        queue.append((x, height - 1))
    for y in range(height):
        queue.append((0, y))
        queue.append((width - 1, y))

    while queue:
        x, y = queue.popleft()
        if (x, y) in seen or not is_checker_pixel(pixels[x, y]):
            continue
        seen.add((x, y))
        pixels[x, y] = (0, 0, 0, 0)
        if x > 0:
            queue.append((x - 1, y))
        if x + 1 < width:
            queue.append((x + 1, y))
        if y > 0:
            queue.append((x, y - 1))
        if y + 1 < height:
            queue.append((x, y + 1))

    return rgba


def process_units(source: Path, output: Path) -> None:
    atlas = Image.open(source).convert("RGBA")
    frames: list[list[Image.Image]] = []
    maximum_width = 1
    maximum_height = 1

    for row in range(3):
        row_frames: list[Image.Image] = []
        top, bottom = cell_bounds(atlas.height, row, 3)
        for column in range(4):
            left, right = cell_bounds(atlas.width, column, 4)
            cell = clear_connected_checkerboard(atlas.crop((left, top, right, bottom)))
            bounds = cell.getbbox()
            if bounds is None:
                raise ValueError(f"No sprite found at row {row}, column {column}")
            frame = cell.crop(bounds)
            maximum_width = max(maximum_width, frame.width)
            maximum_height = max(maximum_height, frame.height)
            row_frames.append(frame)
        frames.append(row_frames)

    cell_width, cell_height = 40, 48
    usable_width, usable_height = 36, 41
    scale = min(usable_width / maximum_width, usable_height / maximum_height)
    output.mkdir(parents=True, exist_ok=True)

    for row, unit_name in enumerate(UNIT_NAMES):
        for column, facing_name in enumerate(FACING_NAMES):
            source_frame = frames[row][column]
            width = max(1, round(source_frame.width * scale))
            height = max(1, round(source_frame.height * scale))
            sprite = source_frame.resize((width, height), Image.Resampling.NEAREST)
            frame = Image.new("RGBA", (cell_width, cell_height), (0, 0, 0, 0))
            frame.alpha_composite(sprite, ((cell_width - width) // 2, cell_height - height - 3))
            frame.save(output / f"{unit_name}_{facing_name}.png", optimize=True)


def process_terrain(source: Path, output: Path, face: str) -> None:
    atlas = Image.open(source).convert("RGB")
    output.mkdir(parents=True, exist_ok=True)

    for column, terrain_name in enumerate(TERRAIN_NAMES):
        left, right = cell_bounds(atlas.width, column, 3)
        texture = atlas.crop((left, 0, right, atlas.height))
        side = min(texture.size)
        x = (texture.width - side) // 2
        # Side atlases keep material transitions (such as the grass turf lip)
        # along their top edge, while top-face art is safest center-cropped.
        y = 0 if face == "side" else (texture.height - side) // 2
        texture = texture.crop((x, y, x + side, y + side))
        texture = texture.resize((32, 32), Image.Resampling.NEAREST)
        texture.save(output / f"{terrain_name}_{face}.png", optimize=True)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("unit_source", type=Path)
    parser.add_argument("terrain_source", type=Path)
    parser.add_argument("asset_root", type=Path)
    parser.add_argument("--terrain-side-source", type=Path)
    args = parser.parse_args()

    process_units(args.unit_source, args.asset_root / "Units")
    process_terrain(args.terrain_source, args.asset_root / "Terrain", "top")
    if args.terrain_side_source is not None:
        process_terrain(args.terrain_side_source, args.asset_root / "Terrain", "side")


if __name__ == "__main__":
    main()

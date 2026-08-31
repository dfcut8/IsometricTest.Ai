# Isometric Pixel-Art Tactical Prototype

A small Godot 4 C# prototype that keeps tactical rules on a square 2D grid while rendering the board as a rotating 3D isometric scene.

## Run

1. Open `project.godot` with the .NET/Mono build of Godot 4.6 or newer.
2. Let Godot build the C# project if prompted.
3. Press **F6/F5** to run the main scene/project.

The project targets .NET 8, which is supported by Godot 4.6 .NET.

## Controls

- **Q** — rotate the camera left by 90 degrees.
- **E** — rotate the camera right by 90 degrees.
- **Mouse move** — highlight the terrain cell under the pointer.
- **Left click a unit** — select it and show its logical coordinate/facing.
- **Left click an empty tile** — move the selected unit directly to that cell.

## Architecture

- `Scripts/Map/` contains the authoritative `MapCell[,]`, elevations, terrain types, grid/world conversion, and logical unit occupancy. It never queries scene nodes or physics.
- `Scripts/Rendering/` turns the logical map into box meshes/colliders and generates the tiny pixel textures.
- `Scripts/Units/` separates `UnitState` (grid coordinate and world-facing direction) from `TacticalUnit`/`UnitVisual` (3D representation).
- `Scripts/Camera/` owns the orthographic camera and its smooth, non-overlapping 0.3-second quarter-turn tweens.
- `Scripts/Input/` performs one camera raycast implementation for all view angles. Terrain colliders carry their immutable logical coordinate.
- `Scripts/Battle.cs` assembles the intentionally small procedural scene into `World/Terrain`, `World/Objects`, `World/Units`, `CameraPivot/Camera3D`, and `UI`.

## Pixel-art assets and rendering

The unit's four 16×24 directional frames, the tree, and the crate are generated at startup by `Scripts/Rendering/PrototypeTextures.cs` using Godot `Image` and `ImageTexture`. There are no downloaded or missing asset files. Every `Sprite3D` uses nearest-neighbor filtering, unshaded rendering, alpha cutout, and a camera-facing billboard.

The internal viewport is 320×180 and the default window is 1280×720. Integer scaling and nearest texture filtering keep the low-resolution presentation crisp.

## Rotation and directional frames

The camera pivot starts over the center of the 10×10 board at a 45-degree isometric yaw. Q/E changes only the view quadrant and tweens the pivot by exactly 90 degrees. Inputs received while that tween is active are ignored.

A unit's `UnitState.Facing` always remains in world coordinates. `UnitVisual` selects the displayed frame with `(worldFacing - cameraQuadrant + 4) % 4`. During a smooth turn, the camera yaw is rounded to the nearest quadrant, so the texture switches once near the halfway point instead of rotating continuously. The Sprite3D remains billboarded only to keep its plane readable.

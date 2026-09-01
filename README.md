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

The fighter, scout, and mage each use four transparent 40×48 directional frames under `Assets/PixelArt/Units`. Grass, dirt, and rock each use separate 32×32 top and side textures under `Assets/PixelArt/Terrain`. The larger generated source sheets are retained under `Assets/PixelArt/Source`, and `Tools/process_generated_art.py` reproducibly crops and normalizes them.

Terrain still uses real 3D box geometry: each cube body uses a darker, material-specific side texture while a separate unshaded plane places the corresponding top texture on its upper face. Every unit `Sprite3D` uses nearest-neighbor filtering, unshaded rendering, alpha cutout, and a camera-facing billboard. The tree and crate remain small procedural pixel textures.

The internal viewport is 320×180 and the default window is 1280×720. Integer scaling and nearest texture filtering keep the low-resolution presentation crisp.

## Rotation and directional frames

The camera pivot starts over the center of the 10×10 board at a 45-degree isometric yaw. Q/E changes only the view quadrant and tweens the pivot by exactly 90 degrees. Inputs received while that tween is active are ignored.

A unit's `UnitState.Facing` always remains in world coordinates. The sprite filenames are view-relative: north is the back view, east/west are the two profiles, and south is the front view. `UnitVisual` selects the displayed frame with `(worldFacing - cameraQuadrant + 4) % 4`. Separate east and west artwork is used instead of mirroring, so asymmetrical weapons and clothing stay on the correct side. During a smooth turn, the camera yaw is rounded to the nearest quadrant, so the texture switches once near the halfway point instead of rotating continuously. The Sprite3D remains billboarded only to keep its plane readable.

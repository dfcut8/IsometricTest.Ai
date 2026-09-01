# Pixel-art source prompts

The built-in image generation tool created both source atlases. The three supplied
Tactics Ogre screenshots were used only as style, scale, palette, and presentation
references; the units and terrain designs are original.

## Unit sheet

```text
Use case: stylized-concept
Asset type: production game sprite sheet for a Godot isometric tactics prototype
Primary request: Create one original pixel-art character sprite sheet containing three distinct human fantasy units, each shown in four cardinal facings.
Input images: Images 1-3 are style, scale, palette, and isometric-game presentation references only; do not copy any named character, costume, map, or exact design.
Subject: Row 1 is an amber/red lightly armored sword fighter; row 2 is a teal-green hooded scout; row 3 is a violet and cream robed mage. Each row has exactly four full-body standing frames ordered North/back view, East/right profile, South/front view, West/left profile.
Style/medium: authentic crisp 16-bit-era Japanese tactical RPG pixel art, hand-placed square pixels, limited cohesive palette, dark selective outlines, compact readable silhouettes, subtle one-pixel highlights, no antialiasing, no smoothing.
Composition/framing: exact 4-column by 3-row sprite atlas; every cell equal size; one character centered on the same baseline in every cell; generous transparent padding; all figures equal scale; no overlaps.
Constraints: genuinely transparent background; no grid lines; no labels; no text; no shadows beyond a tiny optional contact pixel; no logos; no watermark. Preserve clear directional differences in hair, face, hands, weapons, and clothing. Pixel edges must remain hard and axis-aligned.
Avoid: painterly rendering, vector art, gradients, blur, antialiasing, 3D render, UI panels, scenery, copied characters.
```

## Terrain atlas

```text
Use case: stylized-concept
Asset type: production game texture atlas for the top faces of 3D cube terrain tiles in a Godot isometric tactics prototype
Primary request: Create one original pixel-art atlas with exactly three square, orthographic top-down seamless terrain textures.
Input images: Images 1-3 are style, palette, texture-density, and tactical-map references only; do not copy any exact map or design.
Subject: Column 1 is mossy green grass with tiny restrained leaf and soil clusters; column 2 is warm ochre packed dirt with sparse pebble pixels; column 3 is cool gray-green worn stone paving made of irregular square slabs with subtle moss in cracks.
Style/medium: authentic crisp 16-bit-era Japanese tactical RPG environmental pixel art, hand-placed square pixels, limited harmonious earthy palette, clustered dithering, no antialiasing, no smoothing.
Composition/framing: exact 3-column by 1-row atlas; equal square cells; each texture fills its cell edge-to-edge; texture viewed straight down with no perspective and no lighting direction; seamless/tileable on all four edges.
Constraints: opaque artwork; no gutters, borders, labels, grid overlay, objects, plants taller than ground cover, shadows, text, logos, or watermark. Hard axis-aligned pixel edges.
Avoid: painterly rendering, photorealism, gradients, blur, antialiasing, 3D perspective, isometric diamonds, UI elements, copied map layouts.
```

## Terrain side atlas

```text
Use case: stylized-concept
Asset type: production game texture atlas for the vertical side faces of 3D cube terrain tiles in a Godot isometric tactics prototype
Input images: Image 1 is the existing top-face terrain atlas and is a strict style, palette, pixel density, and material reference. Create a new matching side-face atlas; do not modify or reproduce it as top-down art.
Primary request: Create one original pixel-art atlas with exactly three equal square, straight-on orthographic seamless terrain side textures.
Subject: Column 1 is the vertical cut side of a grassy tile: a thin mossy green turf lip along the entire top edge and dark earthy soil below with sparse tiny root and pebble pixel clusters. Column 2 is warm ochre packed earth seen from the side, with restrained horizontal compacted strata and sparse embedded pebble pixels. Column 3 is a cool gray-green weathered stone retaining face made of irregular fitted blocks with subtle dark seams and tiny moss accents.
Style/medium: authentic crisp 16-bit-era Japanese tactical RPG environmental pixel art, matching Image 1's hand-placed square pixels, limited harmonious earthy palette, clustered dithering, no antialiasing, no smoothing. Slightly darker overall than the corresponding top textures so cube depth remains readable.
Composition/framing: exact 3-column by 1-row atlas; all three cells equal square size; each texture fills its cell edge-to-edge; surfaces viewed perfectly straight-on with no perspective; horizontally seamless/tileable within each cell. Keep each material visually distinct at small 32x32 output size.
Constraints: opaque artwork; perfectly clean cell boundaries; no gutters, borders, labels, grid overlay, isolated objects, plants, cast shadows, directional lighting, text, logos, or watermark. Hard axis-aligned pixel edges.
Avoid: painterly rendering, photorealism, gradients, blur, antialiasing, 3D perspective, isometric diamonds, UI elements, large focal objects, copied map layouts.
```

`Tools/process_generated_art.py` removes the generated unit sheet's baked checkerboard,
crops each cell, normalizes the baselines, and reduces the sprites plus both terrain
texture atlases with nearest-neighbor sampling.

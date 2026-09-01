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

`Tools/process_generated_art.py` removes the generated unit sheet's baked checkerboard,
crops each cell, normalizes the baselines, and reduces the sprites and terrain textures
with nearest-neighbor sampling.

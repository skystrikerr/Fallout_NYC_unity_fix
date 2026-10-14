# NPC character collection

50 face-shape variants paired with 50 modeled outfit designs, including the first five approved prototypes. Unity Project 6 contains the review scene at Assets/NPCCollection/NPCCollectionPreview.unity.

Use Previous 10 / Next 10, the character buttons, Show face / Show outfit, the turn slider and the Idle / Look around / Walk review controls. Characters 01–05 preserve the approved first batch; 06–50 add authored clothing combinations for workers, travelers, vault staff, medical staff, civilians and guards.

## What is included

- 50 editable Blender characters and 50 rigged FBX exports in sibling NPCBatch01 through NPCBatch06 folders.
- Separate modeled clothing components with UVs and per-design fabric/color materials.
- Individual face-shape parameters. The 45 additions vary jaw, nose, cheeks, chin, lips, eye spacing, brow and head width, with different skin-material tones.
- Three technical review animation clips per character.
- Portrait, outfit and sampled walking renders; per-batch validation reports; Catalog.json with source paths and FBX hashes.

## Verification

All 50 files passed the recorded bone/vertex checks. The 45 additions were checked at frames 1, 16 and 32 of all three clips, with no unweighted vertices, invalid weight sums, missing bone groups or non-finite animated vertices. Their clothing meshes have UVs. Triangle range including modifiers: 27,420–37,152.

UnityValidation.json records the final 50 prefabs after the fit updates: zero missing bone references or material textures, three clips each, finite baked walk geometry and actual mesh deformation between two sampled walk poses. Unity texture imports were corrected from the project's 2D sprite defaults to regular mipmapped textures with trilinear filtering. Skin material tints use the appropriate color conversion. Final Unity captures are included here. Blender walk contact sheets precede the final neck/shoulder clearance adjustment; the final outfit and portrait renders include it.

## Scope and remaining art work

This is a review collection derived from two approved base bodies and two skin atlases, not 50 independently sculpted and hand-painted humans. Clothing recipes share reusable components. Skin tones are material tints, not newly generated skin atlases. The hair, fabric surface detail and animation clips remain prototypes. Some coat and shoulder intersections still need an art-fitting pass. Generalized outfit swapping, Humanoid retargeting, LODs and crowd draw-call optimization remain work for game integration. The sampled motion checks do not guarantee clearance in every future animation.

The Three.js game has not been ported. Preserve the sibling YoungAdultFaces and UnityCharacterCollection folders for the editable Blender files' texture dependencies. The Unity collection has its own copied texture dependencies.

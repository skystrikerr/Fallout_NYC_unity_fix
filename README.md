# Fallout: New York — Unity port, milestone 1

Open Unity **My project (6)** and play:
`Assets/FalloutPort/Scenes/Vault120.unity`

This is a native Unity scene, not the browser game embedded in Unity.

## Included

- Original Vault 120 geometry, UVs, procedural textures, signs, furnishings, stairs, and birthday-party decoration from `G:/fallout-nyc-port/_js_source`.
- Source collision volumes and native floor collision; 16 automatic doors and the controlled primary vault door.
- Five residents using approved NPCCollection prefabs: Vance, Lucy, Doyle, Dr. Nandakumar, and Tommy.
- First-person movement, keyboard/mouse and gamepad inputs, aiming, hitscan firing, ammunition/reload, holstering, objective guidance, a basic journal, and pause.
- Original 10mm pistol geometry and radroach artwork. New basic ambience, footstep, shooting, and interface sounds.
- A condensed playable introduction: birthday conversation → Doyle's present → three radroaches by the reactor → Zeke's locker → eviction → Overseer's authorization → vault door → exit checkpoint.

## Controls

- Move: WASD / left stick. Look: mouse / right stick.
- Sprint: Shift / left-stick click. Jump: Space / A.
- Interact: E / X. Continue dialogue: E or Enter / A or X.
- Fire: left mouse / right trigger. Aim: right mouse / left trigger.
- Reload: R / Y. Holster: H / B.
- Journal: Tab / View. Pause: Escape / Menu.

Choose New Game from the title menu, then click the Game view to play. Three save slots preserve vault progress across sessions; F5 quick-saves and F9 quick-loads slot 1. Escape opens the pause menu. The Settings menu defaults to 75% 3D rendering while the interface stays at display resolution.

## Validation

`Assets/FalloutPort/Editor/PortValidation.cs` checks actual capsule movement from the bedroom to the atrium, normal NPC interaction, quest stage guards, actual shot hits, ammunition use, primary-door authorization/collision, and traversal through the exit tunnel. Run `PortValidation.Run()` in Play mode for a fresh scene; restart Play afterward because it advances the quest for testing.

Results are saved to `Assets/FalloutPort/Validation.txt`. Unity screenshots are in `Assets/FalloutPort/Previews`. Controller bindings are implemented; physical controller feel still needs a player test.

## Remaining port work

This is the first playable slice, not a complete conversion. The surface/city, full branching dialogue and skill checks, Mr. Super/Mabel, the rest of the quests, inventory/equipment, full combat and enemy AI, creature skeletal animation, first-person arms, voice/music, and final lighting/performance work remain. The exit checkpoint explicitly stops here; it does not pretend Central Park is already available.

The source game and character collection are preserved. New assets and code live in `Assets/FalloutPort`. The project retains its existing 2D renderer; the port camera selects the existing 3D renderer explicitly.

## Reproducibility

Workspace exporter: `work/unity-port/export-vault.cjs` (hidden local Electron asset conversion).
Unity builder: `Assets/FalloutPort/Editor/BuildVaultPort.cs`, menu **Tools → Fallout Port → Build Vault 120**.
Export: `outputs/UnityPort/Vault120/vault120.json`, original texture PNGs, and `export-summary.json`.
Coordinate conversion mirrors source Z into Unity and reverses triangle winding; units remain metres. Vault origin is rebased from X=4000 to zero for precision. Human assets come from the approved NPCCollection, not the original Three.js human meshes.

## Menu, hair, and idle update

- Native title and pause menus, save/load slots, settings, help, and credits.
- Strand ribbon hair replaces the solid scalp caps across all 50 residents; tied styles retain their buns and ponytails.
- Seven additional looping idles per resident: relaxed, look around, weight shift, check wrist, adjust sleeve, guard watch, and conversation. The vault residents choose varied clips, speeds, and starting phases.
- MenuValidation.Run() checks menu navigation, render scale, new-game reset, and save/load round trips. Results: Assets/FalloutPort/MenuValidation.txt.


## Audio and Windows build
Seven supplied music tracks rotate with three-second crossfades and dialogue ducking. The pistol uses pistolsound1.mp3. Remaining supplied effects are imported for future weapon assignments. Master volume controls all audio.

Open with Unity 6000.5.0f1. Git LFS is required: run git lfs pull after cloning. Play Assets/FalloutPort/Scenes/Vault120.unity. The Windows build contains this vault slice; the surface city remains future work.


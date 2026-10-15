# Fallout: New York — native Unity port

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

This is the first playable slice, not a complete conversion. The surface/city, full branching dialogue and skill checks, Mr. Super/Mabel, the rest of the quests, inventory/equipment, full combat and enemy AI, creature skeletal animation, full voice/audio coverage, and final lighting/performance work remain. The exit checkpoint explicitly stops here; it does not pretend Central Park is already available.

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


Windows build verified 2026-10-14: zero build errors. Local executable: C:/Users/camde/Documents/FalloutNYC-Windows/Fallout NYC.exe. Keep the entire Windows build folder together; the executable requires its Data folder and DLLs.


## Vault gameplay expansion — October 15, 2026
- Fixed controller holster/menu routing and gamepad movement when the cursor is unlocked.
- Pistol-specific first-person grip, sleeve, recoil, tracer, muzzle flash and impact marks.
- Enemy attack damage, physical death reactions, health, stimpaks (Q / D-pad Up), sprint AP and recovery menu.
- Three one-time supply caches, caps and an optional timed security-range challenge with rewards.
- Residents wander near their posts, greet nearby players and react to gunfire. Essential quest residents remain protected.
- Survival state and collected rewards persist in saves. The original weapon roster is now imported with individual grip fits; see the Armory update below.
- Verified the vault quest route, live shots at all three range targets, resident movement, controller input regression, and save/load of the new systems. Arm grip reviewed at hip and aim views.

This remains a playable vault prototype, not a production-ready full game. City content, full inventory/equipment, advanced enemy AI and bespoke reload animations remain future work.


## Complete original weapon roster

Open **Armory** from the title or pause menu. Browse the 11 pages and select any of the 44 weapons to equip it at the vault range with practice ammunition. The normal vault quest uses the imported 10mm pistol. Each weapon has its own model, original stats and hand sockets, fitted hand meshes, and one/two-handed support as appropriate. The roster includes ballistic, energy, heavy, improvised, unique, melee and thrown weapons.

Native firing supports automatic/semi-automatic weapons, pellet counts, magazines, reload timing, tracers, moving gun parts, basic projectiles, grenades, mines and melee. Equipped weapon and ammunition persist in saves. Full reload hand choreography, original RPG combat and regular loot/inventory progression remain incomplete.

44 equip/ammo checks, 18 expansion regression checks and 20 vault route checks passed. All 44 hip views were captured for visual review; wrist, swap-overlap and melee sleeve problems were corrected.

See **PORT_STATUS.md** for full-game scope and remaining work. Tools/Porting/source-port-inventory.json inventories the original JavaScript modules. This milestone does not yet add the surface city or later quests.

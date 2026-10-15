# Unity full-game port status — 15 October 2026

This is a playable migration checkpoint, not a completed or production-ready full-game port. The approved Unity human models and outfits are retained. The original Three.js renderer is not embedded: Unity renders native meshes, materials, characters and weapons. Trusted original gameplay scripts execute in-process through Jint without CLR access.

## Implemented in this checkpoint
- Streamed original exterior city, landmarks and interior geometry: 30,356 mesh instances, 886 streaming cells, 368 portal links and source collision volumes. All 13,576 referenced geometry/texture files verified present.
- Vault-to-city and return travel. Existing condensed Vault 120 introduction remains playable.
- Original item definitions and loot tables; 1,711 city containers. All 44 imported weapons occur in city loot. Extra placements are deterministic and recorded in Tools/Porting/extra-world-loot.json.
- Original actor population/AI, native human views using the approved 50-character roster, and 29 imported original creature prefabs with five animation clips each.
- Player damage, enemy projectiles/tracers, corpse looting, native NPC weapon sockets and arm fitting. Human deaths currently use a simple prone pose, not full ragdolls.
- Original quest, dialogue, faction and companion source modules loaded, with native dialogue/journal, skill checks, trading, terminal hacking and door interactions. Loading a module does not establish end-to-end quest parity.
- XP and native skill-point spending; lockpicking doors and containers; persistent vendor inventory/caps, terminal unlocks, quest progress, enemy/corpse state, player inventory and claimed loot.
- Simulation runs on a worker so city rendering is not blocked on each AI update. Cold story initialization is still slow.
- Existing controller support, 75% rendering, soundtrack, 44 weapons, owned inventory and local save slots retained.

## Verified
- Unity compiles without C# errors.
- Native city floor/collision and movement check; interior destination has a solid floor and return portal. Full portal coverage remains untested.
- Original first surface quest rewards its holotape and XP and advances to MQ03 in Unity.
- Offline original simulation exercised at all 72 hostile locations with no adapter errors, up to 212 actors. This is not a navigation or full quest playthrough test.
- Native enemy death/corpse looting and save/load persistence checked.
- Actual merchant purchase changes caps/stock; terminal wrong guess uses an attempt, correct guess unlocks and persists.
- Dialogue, trading, terminal, creature and NPC pistol/rifle screenshots inspected. All NPC weapon poses have not been reviewed.
- Outside death menu and new-game reset checked; city EventSystem remains active.
- Detailed checkpoint evidence is copied into Tools/Porting/Validation.

## Still required for the full port
1. Complete main/side-quest playthroughs, companion behavior, quest set pieces, ending UI, and original full Vault MQ01 rather than the condensed introduction.
2. Full SPECIAL/character creation and perk selection; native armor, condition, limb damage, aid/chem effects, crafting, VATS and complete RPG/player-stat integration.
3. Day/night/weather, radio, holotapes/minigames, original environmental interactions and full audio event coverage.
4. Replace temporary quest terminal/pickup proxy visuals; verify all interior navigation, portals, locks and ownership/stealing rules.
5. Full weapon reload animation, NPC weapon-pose coverage, vault armory balance, performance/memory profiling and complete build QA.

Known limits: some source world features are not yet represented by native views; merchants currently persist stock but do not restock; source RPG special effects are not all applied to the native player. A successful build does not mean these gaps are complete.

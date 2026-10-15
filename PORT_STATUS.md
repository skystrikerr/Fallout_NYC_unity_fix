# Native Unity port status

The target is the original complete game, not only Vault 120. This file separates functioning Unity gameplay from imported assets and remaining source systems.

## Playable and checked
- Vault 120 map, physical doors, condensed birthday/eviction quest, three reactor enemies and exit checkpoint.
- New character collection, resident idle/short-range movement, essential NPC hit reactions.
- Keyboard/controller input, title/pause menus, local saves, soundtrack, 75% world rendering.
- Health/AP/stimpaks, supply caches, reactive range targets, optional range challenge.
- Original 44 weapon models/materials/stats, native prefabs, individual hand sockets and fitted first-person meshes.
- Armory test menu offers all 44. Normal vault quest now uses imported 10mm pistol.
- Automatic/semi-auto firing, pellet count, per-weapon magazines/reload timing, original mechanical shot offsets, basic plasma/explosive projectiles, timed bouncing grenades, proximity mines, basic melee reach/swing.
- Equipped weapon ID persists in saves.

## Still incomplete within weapons
- Full hand-driven reload animations, bolts/cylinders/spin-up and empty-lock behaviors are not fully reproduced.
- Scope overlays, VATS, weapon condition, limb damage, armor mitigation, perks, full original sounds and weapon balance need the RPG/combat port.
- Armory supplies practice ammunition. It is not the real inventory, pickup, merchant or loot progression.
- Visuals are first-pass native imports. Source models retain their original stylized geometry; this is not a replacement realistic asset pack.

## Remaining full-game port
1. Exterior terrain, Central Park, street grid, city/building chunks, original landmarks and POI districts. Preserve source layout; add streaming/culling and collision checks before enabling city exploration.
2. Vault-to-surface transition, subway, dungeons and landmark interiors, matching original portals and named anchors.
3. Full item registry, inventory/ammo pools, aid/apparel, pickups, containers, merchants, barter and crafting.
4. Original quest graph/dialogue/conditions/rewards, main quest after vault, side quests, factions, companions and save migration. Current MQ01 is condensed, not exact parity.
5. Enemy roster, city AI, combat scheduling, navigation, factions and encounters. Current residents only roam locally and three roaches are one-hit enemies.
6. RPG character creation/SPECIAL/skills/perks/XP, armor/resistance, radiation, survival, weapon condition, VATS and related UI.
7. Ambient life, weather/day/night, radio/audio routing, holotapes/minigames and environmental interactions.
8. Content placement, full quest playthroughs, performance/memory budgets, packaging and release QA.

The source inventory JSON records every original src JavaScript file and hash so modules cannot silently disappear from the port scope. An imported source file is not evidence that its behavior works in Unity.

## Weapon validation
- 44/44 equip, fitted-arm presence and ammunition-consumption checks passed.
- All 44 hip-fire views captured and reviewed; fixes made for wrist stretching, overlapping previous weapons and melee sleeve framing.
- 18 controller/survival/save/death/range regression checks passed.
- 20 vault quest/door/collision/audio checks passed.
- Detailed run output is in Unity Logs/WeaponValidation.txt and Assets/FalloutPort/*Validation.txt.

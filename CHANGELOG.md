### 1.5.1

BUGFIXES

- Left Side Start now actually spawns you west of the Act 2 bridge when you enter the act from the act card or respawn from a DeathLink death. Both dropped you on the right side, behind a bridge logic expected you to already be past
- Logic: the four Act 3 Luke file rooms need the Quill again. Their file node only appears once the Archivist has asked to browse a file, which the Quill gates
- Logic: beating Act 2 requires the Act 2 Bridge Repair item when the bridge is randomized, on either start side

### 1.5.0

Requires a matching apworld -- older seeds will not work.

MAJOR CHANGES

- Filler items split per act instead of one shared pool; Act 1 currency survives death
- Any single act can be reset from the Archipelago options menu
- Card packs open next to the rulebook, not at the table. Act 3 Overhaul lets you stand up at any time
- Run state recovery after a force quit or act exit redone: both saves reload from disk and every item sent is replayed. Fixes abandoned battles marking their node beaten, items lost to them, and duplicates on reconnect
- New `releaseOnActCompletion` setting, included in logic

MINOR CHANGES / BUGFIXES

- A consumable check you already took is no longer offered again later in the same node
- The dagger and angler hook reach the run they were sent to instead of the next one
- The oil painting no longer asks for a side deck card you can no longer draw
- Room settings are remembered across connections, in `Archipelago_Inscryption.cfg` (password in plain text)
- Fewer connection failures when joining a large room
- Logic changes: Quill, Mycologists, All Totem Battles, grizzly bosses, wetlands, later woodlands
- Item recovery reworked; every item must declare how it is recovered. Fixes vessel/conduit upgrades and Ourobot lost to an Act 3 reset, Skink and Ant cards lost to a new run, and the bee figurine from a second Progressive Squirrel
- Finding a card gives you that card. The Stinkbug, Stunted Wolf, Fishbot, Lonely Wizbot and Ourobot can appear in choices, packs and the trader, one per deck
- Cards an uncollected check pays for are no longer handed out early
- Fixes: Act 1 mid-battle saves skipping the node, pack rat plus magpie bottle softlock, duplicate items on connect, currency leaking across acts, Act 2 pushing you into Act 3, item log and DeathLink shared between saves, crash while building a new save
- Reset Save Data clears that save's Archipelago data and returns to the Archipelago menu

### 1.4.5

- Fixed items and checks not reliably saving, and a crash that silently lost them
- Fixed Act 2 sigil icon rendering with mixed sigils
- Act 3 Overhaul: holomap marker, battery charging lockout, and the skippable Dredging Room
- Fixed the Act 3 hammer saving as an item, being unable to leave the Act 3 table, the figurine box check card, and filler cards reading "CAN'T BE SACRIFICED."

### 1.4.4

- Bleach trap works, and generates only in Act 2
- Fixed deck size traps, Act 2 dialogue deathlinks, consumable checks reading the wrong checks, in-battle checks marking a node beaten, and Act 3 nodes overwriting Act 1's

### 1.4.3

- Fixed generation failures with multiple Inscryption games, Dagger and Angler Hook ignoring Smaller Backpack, and grizzly bypass logic

### 1.4.2

- Fixed Act 1 progression timing and logic, the dredging room softlock, the Kaycee's Mod record in the safe, and Act 2 sigils affecting other acts

### 1.4.1

- Fixed always-open shortcuts, the wizard satellite check, bottled Boulders and Black Goats becoming squirrels, and consumable checks sending the wrong check

### 1.4.0

- Added `act2_randomize_bridge` and `act3_overhaul`
- `randomize_challenges` now has 2 consumable checks per area plus 3 at the cabin, and hard requirements for grizzlies

### 1.3.2

- Check cards show rare when progression, terrain when filler
- Fixed guaranteed consumable checks, their saving on quit, pack rat sigil duplication, area 4 sending Act 2 checks, the third candle, and many logic bugs

### 1.3.1

- Gems Module grants its side deck instantly; paintings use aquasquirrels when that is your side deck
- Fixed Leshy requiring Progressive Grizzlies, challenges leaking into other acts, and the Free Teeth Skull

### 1.3.0

- Added `randomize_nodes` and `randomize_challenges`, bringing combat logic to Act 1
- Reorganized options into groups

### 1.2.1

- Added four traps, two permanent and two lasting one fight, plus `randomize_once` fixes

### 1.2.0

- Added `act_unlocks` and new `goal` options, a chapter select screen, and an in-game AP settings menu

### 1.1.5

- Added `randomize_sigils: randomize_once`; title screen shows the enabled act; epilogue and `extra_sigils` fixes

### 1.1.4

- Added `extra_sigils`, mixing Act 1, Act 3 and Kaycee's Mod sigils

### 1.1.3

- Added `randomize_vessel_upgrades`

### 1.1.2

- Added `randomize_shortcuts`; fixed Act 3 items arriving 2 IDs late and the lingering hammer

### 1.1.1

- Added `randomize_hammer`

### 1.1.0

- Added options for which acts to play and whether they must be played in order

### 1.0.3

- Fixed the client version string forcing a discontinued client

### 1.0.2

- Updated MultiClient.Net to 6.6.0; Act 2 deathlink crash and deathcard fixes

### 1.0.1

- Updated MultiClient.Net to 6.5.0, fixing disconnects after hinting

### 1.0.0

- Fixed Act 2 battles breaking on start when randomizing sigils

### 0.3.1

- Apworld option support, no Hoarder in Act 2, no epilogue deathlink, grouped epitaph piece fix

### 0.3.0

- New options: skip epilogue, painting check placement, starter-deck-only and all-sigil randomization
- "Randomize Abilities" renamed to "Randomize Sigils"; pools adjusted; several deathlink and Act 3 sigil fixes

### 0.2.2

- Caged wolf always in a randomized deck you own it in; paintings use a bee with the bee figurine
- Fixed the camera replica check, a card item lag spike, and false "failed to apply" items

### 0.2.1

- Fixed single candle deathlink softlocks, an Act 3 transition freeze, and items reapplied on connect

### 0.2.0

- Added save files for playing several multiworlds, Act 1 deathlink behaviour, and epitaph grouping
- Wizard pillar codes, Act 3 custom cards in the pool, and many deck randomization fixes

### 0.1.4

- Fixed broken obol cards, Act 1 campfire buffs, and early gem cards in the Act 3 pool

### 0.1.3

- Grizzly scripted deaths only removed with skip tutorial or deathlink

### 0.1.2

- `randomize_type` works in Act 2 within temple and rarity; card pack, push your luck, currency and Ouroboros fixes

### 0.1.1

- Added skip tutorial; fixed the Act 3 clock clue with randomized codes

### 0.1.0

- Dropped the API dependency for json saving; added Ourobot and the goal setting
- Many deathlink, death card and deck randomization fixes across all acts

### 0.0.2

- Card packs disabled on the Act 2 world map; check cards grant on leaving the screen; death cards in randomized decks

### 0.0.1

- First test build

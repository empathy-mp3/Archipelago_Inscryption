### 1.5.3

Mod-side fixes, an Act 3 logic retune in the apworld, and map fixes in the tracker. No item or
location ids changed, so a 1.5.2 seed still pairs with this mod.

- Being sent Ourobot, the Fishbot or the Lonely Wizbot while in Act 2 no longer rewrites every
  sigil in your Act 3 deck. Those items add to Act 3's deck from whichever act you are in, and
  the check for whether you already held the card loaded that deck while Act 2's rules were in
  force, which rerolled all of it from Act 2's sigil pool and saved the result. The sigils that
  came out have no Act 3 artwork and no rulebook page, so they draw as blank squares and
  right-click opens the wrong page. An Act 3 deck this already happened to is not repaired by
  the fix, since Act 3 builds its deck only once
- Playing a card with an activated sigil in Act 3 no longer locks the battle for good when Act
  3 is played before Act 2. The first activated sigil to reach the board plays a tutorial that
  only exists in Act 2, and the error that left behind kept the battle waiting on a step that
  could never finish -- no card could be played and the bell did nothing

The 1.5.3 apworld and tracker:

- Act 3 purchases are gated on how much money the run can have reached rather than on three of
  four Botopia zones being open. Every currency pickup on the Act 3 map comes to $55 outside
  combat, so each purchase asks for one more open zone than the last: one for the $22 Shop Holo
  Pelt, two for the $26 Nano Armor Generator, and Gaudy Gem Land plus three for the Clock, whose
  last digit is on a wall that only appears once the $25 Holo Brush is bought
- The four tracker markers that sat in empty space now sit on the room each check is taken in:
  the Filthy Corpse World, Gaudy Gem Land and Foul Backwater shortcuts, and the Wizard Tower
  satellite dish
- The three Vessel Upgrades show in all four Act 3 boss rooms rather than on a tile of their
  own. They are one set of checks displayed in four places, so the count stays three
- Tracker item grids drop from 64px to 48px, so the act columns fit a window shorter than about
  1000 points instead of running off the bottom
- The Lonely Wizbot, Fishbot and Ourobot icons are cropped to the card art, since their name
  banners were unreadable at item size

### 1.5.2

Mod-side fixes. The apworld and tracker ship 1.5.2 with no changes of their own, so a
1.5.1 seed pairs with this mod exactly as before.

- The Next button on the save name screen works again, so a new save can be created. Checking
  the name for illegal characters called a string method that is missing from the runtime some
  installs load, and the exception it threw every frame is what left the button dead
- Resetting the save file no longer leaves Act 1 with an oversized deck. The reset deals a
  run before the mod is back in touch with the server, so every card item the server then
  resent was added on top of it. Entering Act 1 now deals that run again
- Items still waiting to be announced are applied before anything reads what they write,
  and the connect screen waits for all of them instead of just the one on screen, so a pile
  of items can no longer land on an act you have already started playing
- Leaving an act through the mod's own exit no longer replays that act's release, since
  that exit saves on its way out and has nothing to revert
- Reconnecting no longer hands out a second copy of a counted item -- currency, packs,
  pelts, upgrades, challenges and traps -- that was never missing

### 1.5.1

No mod-side changes. Version bump to stay in step with the 1.5.1 apworld and tracker,
which fix two logic errors:

- The four Act 3 Luke file rooms need the Quill again. Their file node only appears once
  the Archivist has asked to browse a file, and reaching the Archivist needs the Quill
- Beating Act 2 requires the Act 2 Bridge Repair item when the bridge is randomized, from
  either start side. Two Scrybes are across the bridge whichever side you begin on

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

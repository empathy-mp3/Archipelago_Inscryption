### 1.4.5
 - Fixed a bug where items and checks weren't reliably saved to disk shortly after being received, because the save coroutine was never actually started.
 - Fixed a bug where the Resplendent Bastion Gate item had no reconnect-time recovery if its flag failed to save, unlike most other items.
 - Fixed a bug in Act 2 where the activated-ability button (the clickable energy/bones-cost button on some sigils) could cover up other sigils on the same card when randomize sigils mixed it with others.

### 1.4.4
 - Made it so that bleach trap properly functions in act 2 (and will generate in Act 2 only).
 - Fixed a softlock where it wouldn't add extra cards from deck size traps unless you started Act 2, which let your deck size be larger than the amount of cards you have.
 - Made it impossible to receive a deathlink during Act 2 dialogue.
 - Fixed a bug where the consumable checks were looking at the wrong checks for whether to guarantee a bottle to spawn.
 - Fixed a bug where if you get a check in battle and quit and reload Act 1, you'd be on the battle node as if you'd already beaten it.
 - Fixed a bug where clicking a node in Act 3 would modify Act 1's `currentNodeId`, which would usually place you far below the Act 1 map and softlock you.
 - Slightly reworded the Randomize Challenges option description to be more accurate.

### 1.4.3
 - Fixed a bug where multiple games of Inscryption Beta with different options caused a myriad of issues including generation failures.
 - Fixed a bug where Dagger and Angler Hook weren't properly accounting for Smaller Backpack Challenge.
 - Adjusted the logic for bypassing grizzlies.

### 1.4.2
 - Fixed a bug where two Act 1 items would be progression at the wrong times.
 - Changed up Act 1 logic a little.
 - Fixed a potential softlock with unlocking the dredging room before being allowed to stand up.
 - Fixed the Kaycee's Mod record sometimes appearing in the safe.
 - Fixed a bug where Act 2 sigil randomization would affect other acts.

### 1.4.1
 - Fixed a bug where two of the shortcuts were always open.
 - Fixed a bug where the wizard satellite wouldn't give its check after getting the Resplendent Bastion Gate item.
 - Fixed a bug where Boulders and Black Goats in bottles would always turn into squirrels.
 - Fixed a bug where consumable checks were sending the wrong check.

### 1.4.0
 - Added `act2_randomize_bridge`. The bridge repair is tied to an item instead of scrybes with this option.
 - Added `act3_overhaul`. In addition to randomizing the bridge like the previous option, it also makes the Inspectometer Battery only lock Foul Backwater instead of locking you out of the game, and randomizes the Resplendent Bastion Gate, adding a check for the satellite that normally unlocks it.
 - Changed `randomize_challenges` to have only 2 consumable checks per area, replaced with 3 sphere 1 checks around the cabin.
 - Made hard requirements for beating grizzlies.
 - Various other undocumented fixes.

### 1.3.2
 - Check cards in Act 1 and Act 2 now show as rare if they're progression and terrain if they're filler.
 - Fixed a lot of logic bugs.
 - Guaranteed consumable checks are now given for ones you haven't gotten yet.
 - Consumable checks now properly save their info when quitting to title.
 - Fixed pack rat getting duplicated sigils with `randomize_sigils: randomize_once`.
 - Fixed map area 4 giving consumable checks that send Act 2 checks.
 - Removed some particularly bad vessel upgrades from the sigil pool.
 - Fixed the third candle not showing up after restarting a run.

### 1.3.1
 - Gems Module now gives you a gem side deck instantly, rather than requiring you to retrieve it first.
 - Fixed a bug where Leshy would sometimes logically require Progressive Grizzlies despite not having Grizzly Bosses applied to him.
 - Made the Free Teeth Skull non-functional with randomize challenges.
 - Fixed a bug where challenges would apply to other acts (e.g. Tipped Scales dealing damage on entering an Act 3 battle).
 - Made paintings require aquasquirrels instead of squirrels when that's your side deck.

### 1.3.0
 - Added two big Act 1 options that add combat logic to Act 1:
	- `randomize_nodes`: all upgrade nodes in Act 1 are inoperable until you find the item for them. Adds Goobert's Copy Card Node.
	- `randomize_challenges`: most Kaycee's Mod challenges are ported into Act 1 and disabled by finding their item. Grizzly Bosses is split into 3 "Progressive Grizzlies", and Tipped Scales Challenge has two more copies that each remove a health at the start of each battle.
 - Reorganized options into option groups and added further descriptions for options that were missing clarification.

### 1.2.1
 - Added traps: two permanent (making your deck less consistent) and two temporary (applying for just one fight).
 - Various `randomize_sigils: randomize_once` bug fixes.

### 1.2.0
 - Added `act_unlocks` and new `goal` options. You can now choose how many acts need to be beaten to goal, and whether you start with every act, unlock them in order, or unlock them through items like "Act 1", "Act 2", etc.
 - Added a new chapter select screen and in-game AP settings menu, letting you re-enter Act 1 without restarting your run, toggle deathlink, limit the item log, and send commands.
 - Fixed many bugs across nearly every feature added up to this point.

### 1.1.5
 - Added `randomize_sigils: randomize_once`. Cards get random sigils the moment you see them, and they don't change after that.
 - Made the title screen properly display which act is enabled at any given time.
 - Fixed a bug where finishing Act 3 might never take you to the epilogue.
 - Removed some sigils from `extra_sigils` that didn't work as intended.

### 1.1.4
 - Added `extra_sigils`. Some Act 1 sigils can now appear in Act 3 and vice versa (as well as some Kaycee's Mod sigils), affecting totems, card upgrade nodes, `randomize_sigils`, and `randomize_vessel_upgrades`.
 - Fixed a bug where the epilogue button would show up immediately if every act was enabled, and wouldn't show up otherwise.

### 1.1.3
 - Added `randomize_vessel_upgrades`. Vessel upgrades from bosses (and the conduit upgrade in Resplendent Bastion) can now be randomized in Act 3, giving a random sigil when received, including sigils outside the normal pool like Stinky or Buff When Powered.

### 1.1.2
 - Added `randomize_shortcuts`. The shortcuts in Act 3 can now be randomized.
 - Fixed a major bug where almost every Act 3 item received would instead give the item 2 IDs later.
 - Fixed a bug where the hammer wouldn't go away at the end of battles.

### 1.1.1
 - Added `randomize_hammer`. You can now choose to delete the hammer entirely so you never receive it.
 - Fixed a visual bug where the Archipelago save file would display the wrong number of acts necessary to goal.

### 1.1.0
 - Added options to choose which acts you wish to play, and whether you need to play them in order or in any order.

### 1.0.3
 - Fixed client version string forcing the use of a discontinued client version.

### 1.0.2
 - Updated Archipelago.MultiClient.Net to 6.6.0.
 - Fixed crash that would sometimes occur on a deathlink during act 2.
 - Fixed basic cards in act 2 randomizing when they shouldn't with randomize by type enabled.
 - Fixed deathcard getting skipped if dying normally and the last choice was to skip with "deathlink only" option.

### 1.0.1
 - Updated Archipelago.MultiClient.Net to 6.5.0 which fixes disconnect issues after hinting.

### 1.0.0
 - Fixed a bug that would sometimes cause act 2 battles to break on start when randomizing sigils.

### 0.3.1
 - Aded support for updated options from the latest apworld.
 - The hoarder sigil can no longer appear in act 2 with randomized sigils on as it was not implemented in this act.
 - Death link no longer applies during the epilogue.
 - Fixed a bug that gave all epitaph pieces for free in act 2 if they were grouped as one item.

### 0.3.0
 - Added support for new options:
	- Added option to skip the epilogue.
	- Added option to adjust the 2nd and 3rd painting checks. They can be moved to later spheres or be forced to only contain filler items.
	- Added a new choice for the "Randomize Deck" option to only randomize starter decks.
	- Added a new choice for the "Randomize Sigils" option to randomize all sigils on cards.
 - The "Randomize Abilities" option has been renamed to "Randomize Sigils".
 - Basic cards in act 2 (squirrels, skeletons, mox cards) are no longer randomized when randomizing the deck by type.
 - A message appears after completing acts.
 - Card pools for deck randomization and sigil pools for sigil randomization have been adjusted.
 - The Mycobot card reward from the Mycologists boss is now included in the random card pool when receiving it.
 - Dates in the chapter select screen have been replaced with act titles that turn green when completed.
 - Fixed all candles extinguishing in act 1 combats after receiving deathlink even with the single candle option selected.
 - Fixed multiple deathlink errors and moved to a failsafe approach in case more errors occur during deathlink.
 - Fixed Grimora's ghouls not fighting the player if the Grimora puzzle was completed before fighting them.
 - Fixed modded sigils being randomized in act 3 even with the option disabled when randomizing the deck.
 - Fixed modded sigils not randomizing even with the option enabled when not randomizing the deck.
 - Fixed the Hrokkall card unintentionally appearing in act 1 after starting act 2.

### 0.2.2
 - The caged wolf card will always appear in your deck if you own it when randomizing your deck.
 - The oil painting puzzles in act 1 will now contain a bee instead of a squirrel if you have the bee figurine.
 - Fixed the camera replica check being impossible to get if all 3 of Leshy's subordinates were defeated before ever talking to Leshy in act 2.
 - Fixed a lag spike that would occur when receiving a card item after connecting to the server.
 - Fixed some items that would wrongfully be marked as "failed to apply".

### 0.2.1
 - Single candle deathlink no longer forces a view switch towards the candles if more lives are still remaining.
 - Fixed single candle deathlink soft-locking the game in certain situations.
 - Fixed game freeze during screen transitions in act 3 when the deck is randomized.
 - Fixed certain items that were being reapplied by mistake on connect.

### 0.2.0
 - A new save file system allows you to create and select save files on startup to facilitate playing in multiple multiworlds.
 - Added a new setting for how deathlink behaves in act 1. You can now choose to only lose a single candle when receiving a deathlink and send a deathlink when losing a candle.
 - Added a new setting for how epitaph pieces are randomized. You can now choose to group them in groups of three or group them all as a single item.
 - Wizard pillars in act 2 now have a random code when randomizing codes.
 - Custom built cards in act 3 are now added to the randomization pool when randomizing cards.
 - Moved the femur pedestal further left in the Bone Lord's lair in act 2 to avoid a double pickup glitch.
 - Luke's file entry locations are now consistent with the place they were found in.
 - Fixed custom built cards acting as a card modification which would randomize into another card with the custom card's stats, abilities and cost on top.
 - Fixed randomized cards in act 3 which could end up with more than the maximum amount of four sigils.
 - Fixed act 2 card items not appearing in your collection if received before first starting act 2.
 - Fixed common act 2 cards sometimes randomizing into rare cards when randomizing cards within the same type.
 - Fixed pelt cards in act 1 randomizing into other rare cards when randomizing cards within same type.
 - Fixed the chapter select option for act 3 sending the player in the starting area where they cannot leave if the epilogue was chosen in the past in the chapter select menu.
 - Fixed some issues with item verification.

### 0.1.4
 - Fixed broken obol cards not staying in the randomized deck in act 2 if the obol check wasn't done and the obol object was received.
 - Fixed campfire buffs in act 1 applying to multiple cards in rare occasions with the randomized deck.
 - Fixed a bug that added gems related cards to the randomized deck pool in act 3 if the gems module item was received but not fetched.

### 0.1.3
 - Removed grizzly scripted deaths in act 1 (for real this time). Now only removed if the tutorial is skipped or deathlink is on.

### 0.1.2
 - The randomize type option now works in act 2 within the same temple and rarity.
 - Removed grizzly scripted deaths in act 1.
 - Card packs are no longer available while Leshy displays the starting deck in act 1.
 - You can now push your luck in campfires if the skip tutorial option is enabled.
 - Fixed currency item not applying correctly on every act.
 - Fixed Ouroboros card not appearing with deck randomization in act 1.

### 0.1.1
 - Added skip tutorial setting.
 - Fixed a bug that showed the wrong clock clue in act 3 with randomized codes.

### 0.1.0
 - Removed API dependency.
 - Changed internal saving system to use json instead of the API.
 - Added the Ourobot card to the item pool.
 - Added goal setting.
 - Some items are now double-checked when connecting to a server to prevent potential issues.
 - Cards from the Archipelago item pool can now only appear in randomized decks if unlocked.
 - The map now disappears properly before the optional death card choice in act 1.
 - Receiving a deathlink now waits for the player to unpause.
 - Receiving a deathlink in act 2 now sends the player to the world map.
 - The left and right side of the broken obol now always appear in the randomized deck in act 2 if the obol check isn't completed.
 - The pause button is now disabled while dying from deathlink.
 - Talking cards can now only appear once in randomized decks.
 - Card mods now properly stay when randomizing the deck in act 3.
 - The card pool for deck randomization in act 3 has been expanded.
 - Fixed a bug where the holo map appeared out of the player's view after opening a card pack in act 3.
 - Fixed the card pack pile not appearing in act 3 after acquiring the gems module.
 - Fixed a bug where deathlink wouldn't work in certain areas of act 2.
 - Fixed a bug where the act 1 deck wouldn't reset properly on a new run started right after completing act 3.
 - Fixed a bug where the optional death card choice was given when not dying from deathlink instead of the other way around when that setting was chosen.
 - Fixed a bug where deathcards were empty in randomized decks.
 - Fixed an error that occured when receiving a card pack while the pack pile was visible on screen in act 1 and 3.

### 0.0.2
 - The card pack button is now disabled in the act 2 world map (we weren't lazy, the pack opening UI just doesn't exist in that scene lol).
 - Check cards found around the cabin/factory now only grant the check when the card leaves the screen in an attempt to fix some crashes.
 - Death cards can now be found in randomized decks.
 - Fixed a bug that locked the camera in the wrong room when quitting act 2 in a different room than the entrance.
 - Fixed a bug that reverted some received items when first starting act 2.
 - Fixed a bug that locked the chapter select button after starting act 3.
 - Fixed a bug that prevented card modifications from saving when randomizing the deck.
 - Fixed a bug that showed blank names on the first item received after connecting.

### 0.0.1
 - First test build
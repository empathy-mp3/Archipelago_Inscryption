# Known Issues

Bugs gathered from the Archipelago Discord's #inscryption channel. To be filed as
GitHub issues once issue tracking is enabled on this repo.

_Sourced from Discord discussion, 2026-08-02 to 2026-08-07._

## Check / item sync bugs

### 1. Mid-fight soft-lock on 1.4.5 — can't play cards, End Turn unresponsive
speak22 hit a state where card play and End Turn stopped working entirely during a
fight. A consumable check sent during that broken fight. Exiting and reopening the
game then **skipped the fight** outright.

### 2. Snow Line Battle 2 check failed to send
Reported in the same session as #3.

### 3. Deathlink during card-draft causes soft-lock
Timooni received a deathlink while choosing a card to add to their deck; it let
them take the creature anyway, ran normally through the next fight, then
**soft-locked after winning that fight**. Saust is investigating whether this is
linked to the deathlink mode setting (candle-extinguished vs. instant-death) and
plans a fix in a future beta.

## Process / compatibility notes (not bugs, but relevant caveats)

- Mid-run version swaps between beta releases are **not guaranteed safe** — Saust
  has renamed some checks internally between releases (typo/naming-consistency
  fixes), which can break saves if updating mid-run instead of between runs.
- "Beta" mod/apworld and non-beta mod/apworld are treated as different games —
  mismatching them causes connection failures (already covered in the pinned
  troubleshooting post by Zygan).

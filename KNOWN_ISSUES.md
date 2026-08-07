# Known Issues

Bugs gathered from the Archipelago Discord's #inscryption channel. To be filed as
GitHub issues once issue tracking is enabled on this repo.

_Sourced from Discord discussion, 2026-08-02 to 2026-08-07._

## Connection issues

### 1. WebSocket "Connection timed out" on connect, unresolved for some users
Multiple users (geo9263, Megalo, and at least one other) get:
> Connection timed out. The remote party closed the WebSocket connection without
> completing the close handshake.

when connecting via the mod, even though the Archipelago text client connects fine
to the same room. Standard fixes (disable firewall/antivirus, try a smaller room to
cache datapackages, restart) did **not** resolve it for at least 2 reported cases.
Confirmed to originate from the upstream `Archipelago.MultiClient.Net` library, not
mod-specific — but still worth tracking since it affects mod usability. No root
cause identified yet.

## Check / item sync bugs

### 2. Consumable check silently fails to send once, then works on retry
Swordflare received a "stinkbug" item that didn't register/apply; after starting a
new run, the same check triggered and worked correctly.

### 3. Mid-fight soft-lock on 1.4.5 — can't play cards, End Turn unresponsive
speak22 hit a state where card play and End Turn stopped working entirely during a
fight. A consumable check sent during that broken fight. Exiting and reopening the
game then **skipped the fight** outright.

### 4. Snow Line Battle 2 check failed to send
Reported in the same session as #3.

### 5. Off-by-one check attribution after mod downgrade (1.4.5 → 1.4.4)
After downgrading, beating Snow Line Battle 3 sent the check for **Snow Line
Battle 2** instead of Battle 3.

### 6. Deathlink during card-draft causes soft-lock
Timooni received a deathlink while choosing a card to add to their deck; it let
them take the creature anyway, ran normally through the next fight, then
**soft-locked after winning that fight**. Saust is investigating whether this is
linked to the deathlink mode setting (candle-extinguished vs. instant-death) and
plans a fix in a future beta.

### 7. Hammer duplication / wrong inventory slot bug (save corruption)
Recurring issue prior to 1.4.5. Saust's working theory: on **1.4.4**, if a save is
triggered mid-fight as a result of collecting a bottle check, the save file gets
corrupted and hammers end up in the wrong slots. Believed fixed in 1.4.5+, but
saves already corrupted under 1.4.4 don't self-correct just by upgrading the mod
mid-run — the corruption persists. speak22 hit this exact issue after generating a
world on 1.4.4 and later moving to 1.4.5 mid-run.

## Process / compatibility notes (not bugs, but relevant caveats)

- Mid-run version swaps between beta releases are **not guaranteed safe** — Saust
  has renamed some checks internally between releases (typo/naming-consistency
  fixes), which can break saves if updating mid-run instead of between runs.
- "Beta" mod/apworld and non-beta mod/apworld are treated as different games —
  mismatching them causes connection failures (already covered in the pinned
  troubleshooting post by Zygan).

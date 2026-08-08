# Known Issues

Bugs gathered from the Archipelago Discord's #inscryption channel. To be filed as
GitHub issues once issue tracking is enabled on this repo.

_Sourced from Discord discussion, 2026-08-02 to 2026-08-07._

## Check / item sync bugs

### 1. Snow Line Battle 2 check failed to send
Reported in the same speak22 session as the (now fixed) mid-fight soft-lock, so it
may have been a symptom of that fight never resolving rather than its own bug.

### 2. Deathlink during card-draft causes soft-lock
Timooni received a deathlink while choosing a card to add to their deck; it let
them take the creature anyway, ran normally through the next fight, then
**soft-locked after winning that fight**. Saust is investigating whether this is
linked to the deathlink mode setting (candle-extinguished vs. instant-death) and
plans a fix in a future beta.

### 3. Sometimes my holo pelts dupe themselves when I connect to a slot

## Process / compatibility notes (not bugs, but relevant caveats)

- Mid-run version swaps between beta releases are **not guaranteed safe** — Saust
  has renamed some checks internally between releases (typo/naming-consistency
  fixes), which can break saves if updating mid-run instead of between runs.
- "Beta" mod/apworld and non-beta mod/apworld are treated as different games —
  mismatching them causes connection failures (already covered in the pinned
  troubleshooting post by Zygan).

# Known Issues

Bugs gathered from the Archipelago Discord's #inscryption channel. To be filed as
GitHub issues once issue tracking is enabled on this repo.

_Sourced from Discord discussion, 2026-08-02 to 2026-08-07._

## Check / item sync bugs

### 1. Deathlink during card-draft causes soft-lock
Timooni received a deathlink while choosing a card to add to their deck; it let
them take the creature anyway, ran normally through the next fight, then
**soft-locked after winning that fight**. Saust is investigating whether this is
linked to the deathlink mode setting (candle-extinguished vs. instant-death) and
plans a fix in a future beta.
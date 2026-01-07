using System;
using System.Collections.Generic;
using System.Text;

namespace Archipelago_Inscryption.Archipelago
{
    internal enum RandomizeDeck
    {
        Disable,
        RandomizeType,
        RandomizeAll,
        StarterOnly,
        COUNT
    }

    internal enum RandomizeSigils
    {
        Disable,
        RandomizeAddons,
        RandomizeAll,
        RandomizeOnce,
        COUNT
    }

    internal enum RandomizeHammer
    {
        Vanilla,
        Randomize,
        Remove,
        COUNT
    }

        internal enum RandomizeShortcuts
    {
        Vanilla,
        Randomize,
        Open,
        COUNT
    }
        internal enum RandomizeVesselUpgrades
    {
        Vanilla,
        Randomize,
        RemoveOne,
        COUNT
    }
    internal enum OptionalDeathCard
    {
        Disable,
        Enable,
        EnableOnlyOnDeathLink,
        COUNT
    }

    internal enum Goal
    {
        ActsInOrder,
        ActsAnyOrder,
        COUNT
    }

    internal enum Act1DeathLink
    {
        Sacrificed,
        CandleExtinguished,
        COUNT
    }

    internal enum EpitaphPiecesRandomization
    {
        AllPieces,
        Groups,
        AsOneItem,
        COUNT
    }
}

using DiskCardGame;
using System;
using System.Collections.Generic;

namespace Archipelago_Inscryption.Components
{
    // One of the cards offered when a pack is opened in the cabin. The base class already does
    // the close-up, the dialogue and adding to the current deck, so this only reports the choice.
    internal class PackChoiceCard : DiscoverableCardInteractable
    {
        // Fires before the discovery sequence starts, which is the only point at which the pile
        // can release its own player lock: the sequence captures MoveLocked and restores it.
        internal Action<PackChoiceCard> onSelected;

        internal Action<PackChoiceCard> onTaken;

        // Set false on the losing cards so a second choice cannot be made mid-discovery.
        internal bool choosable = true;

        internal void SetCard(CardInfo info)
        {
            cardsToUnlock = new List<CardInfo> { info };
            onDiscoverText = info.description;
            storyEvent = StoryEvent.NUM_EVENTS;
            requireStoryEventToAddToDeck = false;
        }

        public override void OnCursorSelectStart()
        {
            if (!choosable) return;

            onSelected?.Invoke(this);

            base.OnCursorSelectStart();
        }

        public override void UnlockObject()
        {
            base.UnlockObject();

            onTaken?.Invoke(this);
        }
    }
}

using DiskCardGame;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Archipelago_Inscryption.Components
{
    // One of the cards offered when a pack is opened in the cabin. The base class already does
    // the close-up, the dialogue and adding to the current deck, so this only reports the choice.
    internal class PackChoiceCard : DiscoverableCardInteractable
    {
        // The offset SelectableCardArray gives an inspected card at a choice node. A card here
        // stands facing the player rather than lying flat, so it reads as lifting and coming out.
        internal static readonly Vector3 HOVER_OFFSET = new Vector3(0f, 0.05f, -0.2f);
        internal const float HOVER_SPEED = 20f;

        // Driven directly, since the card's own interactable is switched off so that its collider
        // cannot take the cursor from this one.
        private SelectableCard card;

        // Fires before the discovery sequence starts, which is the only point at which the pile
        // can release its own player lock: the sequence captures MoveLocked and restores it.
        internal Action<PackChoiceCard> onSelected;

        internal Action<PackChoiceCard> onTaken;

        // Set false on the losing cards so a second choice cannot be made mid-discovery.
        internal bool choosable = true;

        internal void SetCard(SelectableCard card, CardInfo info)
        {
            this.card = card;
            cardsToUnlock = new List<CardInfo> { info };
            onDiscoverText = info.description;
            storyEvent = StoryEvent.NUM_EVENTS;
            requireStoryEventToAddToDeck = false;
        }

        public override void OnCursorEnter()
        {
            base.OnCursorEnter();

            if (!choosable || card == null) return;

            card.Anim.PlayRiffleSound();
            card.SetLocalPosition(HOVER_OFFSET, HOVER_SPEED);
        }

        public override void OnCursorExit()
        {
            base.OnCursorExit();

            if (card == null) return;

            card.SetLocalPosition(Vector3.zero, HOVER_SPEED);
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

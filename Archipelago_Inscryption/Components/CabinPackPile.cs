using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Helpers;
using Archipelago_Inscryption.Utils;
using DiskCardGame;
using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Archipelago_Inscryption.Components
{
    // The pack pile, sitting beside the rulebook: Act 1's on the cabin shelf, Act 3's on the
    // factory wall. Both hang off the object the game itself calls CabinRulebook. Unlike the old
    // table pile this is a permanent scene object, built once when the room loads and rebuilt in
    // place as packs arrive and are spent, so there is no per-view spawn and teardown to get wrong.
    internal class CabinPackPile : MainInputInteractable
    {
        // Placement relative to the rulebook, the one landmark with a known transform in each
        // room. For Act 1 negative Z runs along the shelf away from the book, past the lantern.
        internal static readonly Vector3 PILE_OFFSET_ACT1 = new Vector3(0f, -0.25f, -3.5f);
        internal static readonly Vector3 PILE_OFFSET_ACT3 = new Vector3(0f, 0f, -2.5f);
        internal const float PACK_SPACING = 0.06f;
        internal const float HOVER_RISE = 0.05f;


        // The opened cards are laid out in front of the camera rather than on the shelf, using the
        // same framing the vanilla close-up uses, so they read as cards being offered to you.
        internal const float CHOICE_DISTANCE = 2.4f;
        internal const float CHOICE_HEIGHT = 0f;
        internal const float CHOICE_SPACING = 1.15f;
        // The card prefab's own 90 degree X rotation is what makes a card lie flat, and
        // ResetTransform strips it, so in holder space the card's face normal is -Z. Facing the
        // player therefore means pointing the holder away from the camera, with no tilt term.
        internal const float CHOICE_LEAN = 0f;
        internal const float CHOICE_SCALE = 0.7114f;

        // Mirrors the table pile's opening: the pack lifts and turns, plays its open animation,
        // and the cards come out of it rather than appearing where they land.
        internal static readonly Vector3 PACK_LIFT = new Vector3(0f, 0.25f, 0f);
        // Positive yaw turns the pack's face toward the player as it lifts. The old table pile
        // used -90, but it hung off a parent already rotated 90, so the sign flips here. Act 3's
        // packs are flat card backs, where the same turn is just a spin in place, so they only
        // lift.
        internal static readonly Vector3 PACK_TURN = new Vector3(0f, 90f, 0f);
        internal const float REVEAL_STAGGER = 0.1f;
        internal const float REVEAL_TIME = 0.3f;

        // The shelf lantern only reaches part of the row, so the cabin's hand light is brightened
        // while the cards are on offer, the way vanilla sequencers adjust it. Moving the cards to
        // the FirstPersonLighting layer instead does not work: that layer has no light of its own
        // (FirstPersonLight culls to everything), so it only costs them the room's lighting.
        internal const float HAND_LIGHT_INTENSITY_SCALE = 2f;
        internal const float HAND_LIGHT_RANGE_SCALE = 1.4f;
        internal const float HAND_LIGHT_FADE = 0.3f;


        internal static CabinPackPile instance;

        private readonly List<GameObject> packs = new List<GameObject>();
        private readonly List<PackChoiceCard> choices = new List<PackChoiceCard>();

        private Transform rulebook;
        private CabinInteractable rulebookInteractable;
        private CardPile part3Pile;
        private int act;
        private float handLightIntensity;
        private float handLightRange;
        private bool handLightBoosted;
        private bool opening;

        internal static void Create(Transform cabinRulebook, int act)
        {
            if (instance != null) return;

            GameObject pileObject = new GameObject("ArchipelagoPackPile");
            pileObject.transform.SetParent(cabinRulebook.parent);
            pileObject.transform.localPosition = cabinRulebook.localPosition
                + (act == 3 ? PILE_OFFSET_ACT3 : PILE_OFFSET_ACT1);
            pileObject.transform.localRotation = cabinRulebook.localRotation;
            pileObject.transform.localScale = cabinRulebook.localScale;

            BoxCollider collider = pileObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.2f, 0.4f, 2.2f);

            instance = pileObject.AddComponent<CabinPackPile>();
            instance.rulebook = cabinRulebook;
            instance.rulebookInteractable = cabinRulebook.GetComponent<CabinInteractable>();
            instance.act = act;

            if (act == 3)
            {
                GameObject pileModel = Instantiate(AssetsManager.diskCardPilePrefab, pileObject.transform);
                pileModel.transform.ResetTransform();

                instance.part3Pile = pileModel.GetComponent<CardPile>();
                // The pile is an interactable in its own right; this object handles the cursor.
                instance.part3Pile.SetEnabled(false);
            }

            instance.Rebuild();
        }

        // Rebuilds the visible stack from the current pack count, and disables the collider when
        // there is nothing to open. Safe to call whenever the count changes.
        internal void Rebuild()
        {
            foreach (GameObject pack in packs)
            {
                if (pack != null) Destroy(pack);
            }
            packs.Clear();

            int available = ArchipelagoData.Data.PacksAvailable(act);

            if (act == 3)
            {
                // Let the card pile build its own stack, then track the cards it made so hovering
                // and opening work the same as they do for Act 1's packs.
                part3Pile.DestroyCardsImmediate();
                part3Pile.CreateCards(available, 0f);

                foreach (Transform card in part3Pile.cards)
                {
                    packs.Add(card.gameObject);
                }

                // The pile and its cards carry their own colliders, which sit on top of this
                // object's and would swallow the cursor before it ever reached the pile.
                foreach (Collider collider in part3Pile.GetComponentsInChildren<Collider>(true))
                {
                    collider.enabled = false;
                }
            }
            else
            {
                for (int i = 0; i < available; i++)
                {
                    GameObject pack = Instantiate(AssetsManager.cardPackPrefab, transform);
                    pack.transform.localPosition = new Vector3(0f, PACK_SPACING * i, 0f);
                    pack.transform.localRotation = Quaternion.identity;
                    packs.Add(pack);
                }
            }

            GetComponent<BoxCollider>().enabled = available > 0 && !opening;
        }

        public override void OnCursorEnter()
        {
            if (opening || packs.Count == 0 || !PlayerIsInFront()) return;

            Tween.LocalPosition(packs[packs.Count - 1].transform, TopPackBase() + Vector3.up * HOVER_RISE,
                0.1f, 0, Tween.EaseOut);
        }

        public override void OnCursorExit()
        {
            if (opening || packs.Count == 0) return;

            Tween.LocalPosition(packs[packs.Count - 1].transform, TopPackBase(), 0.1f, 0, Tween.EaseOut);
        }

        public override void OnCursorSelectEnd()
        {
            if (opening || packs.Count == 0 || !PlayerIsInFront()) return;

            // Rolled before the pack is spent, so a generator that cannot offer anything leaves
            // the pack in the pile rather than consuming it for nothing.
            List<CardInfo> cards = RandomizerHelper.RollPackCards(act, 3);
            if (cards.Count == 0) return;

            opening = true;
            GetComponent<BoxCollider>().enabled = false;
            SetPlayerHeld(true);
            ArchipelagoData.Data.SpendPack(act);

            StartCoroutine(OpenPackSequence(cards));
        }

        // The opened cards are placed relative to the camera, so opening the pile from off to the
        // side lays them out through whatever the player is facing. The rulebook beside it already
        // states which zones and look directions it may be used from, so the pile borrows those
        // rather than inventing a distance and an angle.
        private bool PlayerIsInFront()
        {
            return rulebookInteractable != null && rulebookInteractable.PrerequisitesMet;
        }

        private float PackSpacing => act == 3 ? part3Pile.cardYSpacing : PACK_SPACING;

        private Vector3 TopPackBase()
        {
            return new Vector3(0f, PackSpacing * (packs.Count - 1), 0f);
        }

        private IEnumerator OpenPackSequence(List<CardInfo> cards)
        {
            GameObject topPack = packs[packs.Count - 1];
            packs.RemoveAt(packs.Count - 1);
            // Also drop it from the pile's own list, so its next rebuild does not touch a
            // destroyed card.
            if (act == 3) part3Pile.cards.Remove(topPack.transform);

            Tween.LocalPosition(topPack.transform, topPack.transform.localPosition + PACK_LIFT,
                0.2f, 0, Tween.EaseOut);
            if (act != 3)
            {
                Tween.LocalRotation(topPack.transform, topPack.transform.localEulerAngles + PACK_TURN,
                    0.2f, 0, Tween.EaseOut);
            }

            // Only the Act 1 pack model has an open animation, so Act 3 skips both the
            // animation and the time set aside for it, and waits only for the lift.
            if (act == 3)
            {
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                yield return new WaitForSeconds(0.35f);

                Animator animator = topPack.GetComponentInChildren<Animator>();
                if (animator != null) animator.Play("open", 0, 0f);

                yield return new WaitForSeconds(0.5f);
            }

            // Cards fly out of the pack to the spots they were built at.
            Vector3 origin = topPack.transform.position;
            SetChoiceLighting(true);

            for (int i = 0; i < cards.Count; i++)
            {
                PackChoiceCard choice = CreateChoiceCard(cards[i], i, cards.Count);
                Vector3 target = choice.transform.position;

                choice.transform.position = origin;
                Tween.Position(choice.transform, target, REVEAL_TIME, REVEAL_STAGGER * i, Tween.EaseOut);

                choices.Add(choice);
            }

            Destroy(topPack);

            yield return new WaitForSeconds(REVEAL_TIME + REVEAL_STAGGER * cards.Count);

            foreach (PackChoiceCard choice in choices)
            {
                if (choice != null) choice.choosable = true;
            }
        }

        // Cards are placed in world space in front of the camera, unparented, so the close-up that
        // DiscoverableObjectInteractable runs on click starts from the same framing.
        private PackChoiceCard CreateChoiceCard(CardInfo info, int index, int total)
        {
            Transform camera = Singleton<ViewManager>.Instance.CameraParent;

            GameObject holder = new GameObject("ArchipelagoPackCard_" + info.name);
            holder.transform.localScale = Vector3.one * CHOICE_SCALE;

            float spread = CHOICE_SPACING * (index - (total - 1) / 2f);
            holder.transform.position = camera.position
                + camera.forward * CHOICE_DISTANCE
                + camera.right * spread
                + Vector3.up * CHOICE_HEIGHT;

            Vector3 awayFromCamera = camera.forward;
            awayFromCamera.y = 0f;
            holder.transform.rotation = Quaternion.LookRotation(awayFromCamera, Vector3.up)
                * Quaternion.Euler(CHOICE_LEAN, 0f, 0f);

            holder.AddComponent<BoxCollider>().size = new Vector3(1.2f, 0.2f, 1.6f);

            GameObject cardObject = Instantiate(act == 3
                ? AssetsManager.selectableDiskCardPrefab
                : AssetsManager.selectableCardPrefab, holder.transform);
            cardObject.transform.ResetTransform();
            cardObject.GetComponent<SelectableCard>().SetInfo(info);


            PackChoiceCard choice = holder.AddComponent<PackChoiceCard>();
            choice.SetCard(info);
            // Enabled once the cards have finished flying out, so a click cannot land mid-reveal.
            choice.choosable = false;
            choice.closeUpDistance = CHOICE_DISTANCE;
            choice.closeUpVerticalOffset = CHOICE_HEIGHT;
            // The close-up reparents the card to the camera before applying these, so they are
            // camera-local: zero means squarely facing the player.
            choice.closeUpEulers = Vector3.zero;
            choice.onSelected = OnChoiceSelected;
            choice.onTaken = OnChoiceTaken;

            return choice;
        }

        // The losing cards go now, but the chosen one is left alone: its discovery sequence runs
        // on that object and destroys it itself at the end, so removing it here would kill the
        // coroutine mid-dialogue and strand the cursor disabled.
        private void OnChoiceSelected(PackChoiceCard taken)
        {
            foreach (PackChoiceCard choice in choices)
            {
                if (choice == null) continue;
                choice.choosable = false;
                if (choice != taken) Destroy(choice.gameObject);
            }
            choices.Clear();

            // Released before the discovery sequence captures MoveLocked, so it restores to free.
            SetPlayerHeld(false);
            SetChoiceLighting(false);
        }

        private void OnChoiceTaken(PackChoiceCard taken)
        {
            opening = false;
            Rebuild();

            SaveManager.SaveToFile(false);
            ArchipelagoData.SaveToFile();
        }

        // Brightens the room's hand light so the whole row reads, and puts it back afterwards.
        // The original values are captured on the way up so a restore cannot drift.
        private void SetChoiceLighting(bool boosted)
        {
            ExplorableAreaManager area = Singleton<ExplorableAreaManager>.Instance;
            if (area == null || area.HandLight == null) return;

            Light light = area.HandLight;

            if (boosted)
            {
                if (handLightBoosted) return;

                handLightIntensity = light.intensity;
                handLightRange = light.range;
                handLightBoosted = true;

                Tween.LightIntensity(light, handLightIntensity * HAND_LIGHT_INTENSITY_SCALE,
                    HAND_LIGHT_FADE, 0f, Tween.EaseInOut);
                Tween.LightRange(light, handLightRange * HAND_LIGHT_RANGE_SCALE,
                    HAND_LIGHT_FADE, 0f, Tween.EaseInOut);
            }
            else
            {
                if (!handLightBoosted) return;

                handLightBoosted = false;

                Tween.LightIntensity(light, handLightIntensity, HAND_LIGHT_FADE, 0f, Tween.EaseInOut);
                Tween.LightRange(light, handLightRange, HAND_LIGHT_FADE, 0f, Tween.EaseInOut);
            }
        }

        // A pack that is open has to be resolved, so the player neither walks nor turns away
        // until a card is taken. The cursor is unaffected, so the cards stay selectable.
        private void SetPlayerHeld(bool held)
        {
            FirstPersonController controller = Singleton<FirstPersonController>.Instance;
            if (controller == null) return;

            controller.MoveLocked = held;
            controller.LookLocked = held;
        }

        private void OnDestroy()
        {
            // Torn down mid-choice: put the light back immediately, since a tween cannot finish.
            if (handLightBoosted)
            {
                ExplorableAreaManager area = Singleton<ExplorableAreaManager>.Instance;
                if (area != null && area.HandLight != null)
                {
                    area.HandLight.intensity = handLightIntensity;
                    area.HandLight.range = handLightRange;
                }

                handLightBoosted = false;
            }

            if (instance == this) instance = null;
        }
    }
}

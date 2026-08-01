using Archipelago_Inscryption.Archipelago;
using DiskCardGame;
using Pixelplacement;
using System.Collections;
using UnityEngine;

namespace Archipelago_Inscryption.Components
{
    internal class DiscoverableCheckInteractable : DiscoverableCardInteractable
    {
        internal APCheck check;

        internal SelectableCard card;

        // How far back to pull the close-up. Zero leaves vanilla's framing untouched.
        internal float closeUpPullback = 0f;

        private Transform pulledCamera;
        private Vector3 pulledCameraOrigin;

        public override void UnlockObject()
        {
            StartCoroutine(UnlockAfterDiscard());
        }

        public override IEnumerator PreDialogueSequence()
        {
            PullBackForCloseUp();
            yield break;
        }

        public override void OnCursorSelectStart()
        {
            if (!requireStoryEventToAddToDeck || StoryEventsData.EventCompleted(requiredStoryEvent)) 
            {
                base.OnCursorSelectStart();
            }
            else
            {
                card.Anim.PlayRiffleSound();
            }
        }

        // Moves the camera AND the card back by one shared vector, so the close-up keeps vanilla's
        // framing but sits further from whatever the card would otherwise clip through.
        private void PullBackForCloseUp()
        {
            if (closeUpPullback <= 0f) return;

            ViewManager viewManager = Singleton<ViewManager>.Instance;
            if (viewManager == null || viewManager.CameraParent == null) return;

            pulledCamera = viewManager.CameraParent;
            pulledCameraOrigin = pulledCamera.localPosition;

            Vector3 shift = -pulledCamera.forward * closeUpPullback;

            // The point DiscoverySequence just aimed the card at, recomputed because it is a fixed
            // world position we cannot read back off the running tween.
            Vector3 vanillaCardTarget = pulledCamera.position
                + pulledCamera.forward * closeUpDistance
                + Vector3.up * closeUpVerticalOffset;

            // Replaces vanilla's card tween rather than racing it: Pixelplacement only stops tweens
            // already running, so the one created later wins.
            MoveCard(vanillaCardTarget + shift);
            MoveCamera(pulledCamera.position + shift);
        }

        private void MoveCard(Vector3 worldTarget)
            => Tween.Position(transform, worldTarget, TRANSITION_DURATION, 0f, Tween.EaseInOut);

        // Local space, matching ViewManager's own view tweens and the restore in UnlockAfterDiscard.
        // Pixelplacement treats local and world tweens as separate, so these must not be mixed.
        private void MoveCamera(Vector3 worldTarget)
        {
            Transform parent = pulledCamera.parent;
            Vector3 localTarget = parent == null ? worldTarget : parent.InverseTransformPoint(worldTarget);
            Tween.LocalPosition(pulledCamera, localTarget, TRANSITION_DURATION, 0f, Tween.EaseInOut);
        }

        private IEnumerator UnlockAfterDiscard()
        {
            yield return new WaitUntil(() => Discovering);
            yield return new WaitUntil(() => !Discovering);

            if (pulledCamera != null)
            {
                Tween.LocalPosition(pulledCamera, pulledCameraOrigin, TRANSITION_DURATION, 0f, Tween.EaseInOut);
                pulledCamera = null;
            }

            ArchipelagoManager.SendCheck(check);
        }
    }
}

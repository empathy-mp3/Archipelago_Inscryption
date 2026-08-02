using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Helpers;
using DiskCardGame;
using System.Collections.Generic;
using System.Linq;
using GBC;
using UnityEngine;

namespace Archipelago_Inscryption.Components
{
    internal class ArchipelagoOptionsMenu : ManagedBehaviour
    {
        private OptionsUI menu;
        private IncrementalField itemLogField;
        private IncrementalField deathLinkField;
        private InputField commandField;
        private IncrementalField actResetField;
        private GenericUIButton actResetButton;
        private List<int> resettableActs;
        private int resetClicks;

        internal void Setup(OptionsUI menu)
        {
            this.menu = menu;

            itemLogField = Instantiate(menu.resolutionField, transform);
            itemLogField.name = "IncrementalField_ItemLog";
            itemLogField.transform.localPosition = menu.masterVolumeSlider.transform.localPosition;
            itemLogField.transform.Find("Title").GetComponent<PixelText>().SetText("Item Log");
            itemLogField.valueChanged = null;
            itemLogField.AssignTextItems(["Disabled", "Yours Only", "All Items"]);
            itemLogField.ShowValue((int)ArchipelagoData.itemLogMode, true);
            itemLogField.valueChanged = ItemLogChanged;

            deathLinkField = Instantiate(menu.resolutionField, transform);
            deathLinkField.name = "IncrementalField_DeathLink";
            deathLinkField.transform.localPosition = menu.musicVolumeSlider.transform.localPosition;
            deathLinkField.transform.Find("Title").GetComponent<PixelText>().SetText("Death Link");
            deathLinkField.valueChanged = null;
            if (ArchipelagoOptions.enableAct1)
            {
                deathLinkField.AssignTextItems(["Default", "Disabled", "One Candle", "End Run"]);
            }
            else
            {
                deathLinkField.AssignTextItems(["Default", "Disabled", "Enabled"]);
            }
            deathLinkField.ShowValue((int)ArchipelagoData.deathLinkOverride, true);
            deathLinkField.valueChanged = DeathLinkChanged;

            commandField = UIHelper.CreateInputField(menu, transform, "InputField_Command", "Send Command...", "", menu.applyGraphicsButton.transform.localPosition.y, 100);
            commandField.OnSubmit += CommandSubmitted;
            commandField.gameObject.layer = deathLinkField.rightButton.gameObject.layer;

            CreateActResetControls();
        }
        
        // An act selector plus one full-width button. Three side-by-side buttons cannot fit: they
        // are clones of RESET SAVE DATA, which spans the panel.
        private void CreateActResetControls()
        {
            resettableActs = new List<int>();
            for (int act = 1; act <= 3; act++)
                if (ActResetHelper.CanReset(act)) resettableActs.Add(act);

            if (resettableActs.Count == 0) return;

            actResetField = Instantiate(menu.resolutionField, transform);
            actResetField.name = "IncrementalField_ResetAct";
            actResetField.transform.localPosition = menu.soundVolumeSlider.transform.localPosition;
            actResetField.transform.Find("Title").GetComponent<PixelText>().SetText("Reset Act");
            actResetField.valueChanged = null;
            actResetField.AssignTextItems(resettableActs.Select(act => "Act " + act).ToList());
            actResetField.ShowValue(0, true);
            actResetField.valueChanged = SelectedActChanged;

            actResetButton = Instantiate(menu.resetSaveDataButton, transform);
            actResetButton.name = "Button_ResetAct";
            // resetSaveDataButton and applyGraphicsButton share a row (y -0.9), and commandField
            // already sits there. setLanguageButton's row is the free one in this tab.
            actResetButton.transform.localPosition = menu.setLanguageButton.transform.localPosition;
            actResetButton.CursorSelectEnded += _ => ResetButtonPressed();
            UpdateResetButtonText();
        }

        // Changing target abandons a part-made confirmation rather than carrying it across acts.
        private void SelectedActChanged(int value)
        {
            resetClicks = 0;
            UpdateResetButtonText();
        }

        private int SelectedAct => resettableActs[Mathf.Clamp(actResetField.Value, 0, resettableActs.Count - 1)];

        // Mirrors the neighbouring RESET SAVE DATA button: the label escalates and only the last
        // click acts, so the warning cannot be clicked past by accident.
        private void ResetButtonPressed()
        {
            MenuController.PlayMenuCrunchSound(true);

            resetClicks++;

            if (resetClicks <= 2)
            {
                UpdateResetButtonText();
                return;
            }

            int act = SelectedAct;
            ActResetHelper.ResetAct(act);

            // Vanilla's reset button disables itself because the save is gone and the game leaves
            // for the start screen. This one stays put, so re-arm it for the next act.
            resetClicks = 0;
            SetResetButtonText("ACT " + act + " RESET");
        }

        private void UpdateResetButtonText()
        {
            switch (resetClicks)
            {
                case 0: SetResetButtonText("RESET ACT " + SelectedAct); break;
                case 1: SetResetButtonText("REALLY RESET IT"); break;
                default: SetResetButtonText("ERASE ACT " + SelectedAct); break;
            }
        }

        private void SetResetButtonText(string text)
        {
            PixelText label = actResetButton.GetComponentInChildren<PixelText>();
            if (label != null) label.SetText(text);
        }

        internal void ItemLogChanged(int value)
        {
            ArchipelagoData.itemLogMode = (ItemLogMode)value;
        }

        internal void DeathLinkChanged(int value)
        {
            ArchipelagoData.deathLinkOverride = (DeathLinkOverride)value;
            if (ArchipelagoData.DeathLink)
            {
                DeathLinkManager.DeathLinkService.EnableDeathLink();
            }
            else
            {
                DeathLinkManager.DeathLinkService.DisableDeathLink();
            }
        }

        internal void CommandSubmitted(string text)
        {
            ArchipelagoClient.session.Say(text);
            commandField.Text = "";
        }
    }
}

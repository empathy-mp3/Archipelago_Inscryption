using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Helpers;
using DiskCardGame;
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

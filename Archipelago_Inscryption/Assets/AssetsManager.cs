using System.Collections.Generic;
using UnityEngine;

namespace Archipelago_Inscryption.Assets
{
    internal static class AssetsManager
    {
        private static AssetBundle assetBundle;

        internal static Sprite archiSettingsTabSprite;
        internal static Sprite inputFieldSprite;
        internal static Sprite[] packButtonSprites;
        internal static Sprite editedNatureFloorSprite;
        internal static Sprite cardPortraitSprite;
        internal static Sprite cardPixelPortraitSprite;
        internal static Sprite wizardTowerLensClueSprite;
        internal static Sprite wizardTowerSubmergeClueSprite;

        internal static Sprite menuCardAct1Locked;
        internal static Sprite menuCardAct1NewRun;
        internal static Sprite menuCardAct1Continue;
        internal static Sprite menuCardAct1Complete;
        internal static Sprite menuCardAct2Locked;
        internal static Sprite menuCardAct2Start;
        internal static Sprite menuCardAct2Continue;
        internal static Sprite menuCardAct2Complete;
        internal static Sprite menuCardAct3Locked;
        internal static Sprite menuCardAct3Start;
        internal static Sprite menuCardAct3Continue;
        internal static Sprite menuCardAct3Complete;
        internal static Sprite menuCardAct4;

        internal static Texture2D boonTableTex;
        internal static Texture2D[] smallClockClueTexs;
        internal static Texture2D[] factoryClockClueTexs;

        internal static List<Texture2D> lockedNodeFrames;

        internal static GameObject cardPackPrefab;
        internal static GameObject selectableCardPrefab;
        internal static GameObject selectableDiskCardPrefab;
        internal static GameObject archipelagoUIPrefab;
        internal static GameObject cardChoiceHoloNodePrefab;
        internal static GameObject clockCluesPrefab;
        internal static GameObject smallClockCluePrefab;
        internal static GameObject gbcSafeCluePrefab;
        internal static GameObject saveEntryPrefab;

        internal static Mesh checkCardHoloNodeMesh;

        internal static void LoadAssets()
        {
            assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.archiassets);

            if (!assetBundle)
            {
                ArchipelagoModPlugin.Log.LogError("The asset bundle couldn't be loaded.");

                return;
            }

            Texture2D archipelagoTabTex = assetBundle.LoadAsset<Texture2D>("ArchipelagoTab");
            Texture2D inputFieldTex = assetBundle.LoadAsset<Texture2D>("InputFieldImage");
            Texture2D editedNatureFloorTex = assetBundle.LoadAsset<Texture2D>("nature_temple_floor_edited");
            Texture2D cardPortraitTex = assetBundle.LoadAsset<Texture2D>("archi_portrait");
            Texture2D cardPixelPortraitTex = assetBundle.LoadAsset<Texture2D>("archi_portrait_gbc");

            archiSettingsTabSprite = GenerateSprite(archipelagoTabTex);
            inputFieldSprite = GenerateSprite(inputFieldTex);
            editedNatureFloorSprite = GenerateSprite(editedNatureFloorTex);
            cardPortraitSprite = GenerateSprite(cardPortraitTex);
            cardPixelPortraitSprite = GenerateSprite(cardPixelPortraitTex);

            packButtonSprites = assetBundle.LoadAssetWithSubAssets<Sprite>("GBCCardPackButton");

            var wizardClues = assetBundle.LoadAssetWithSubAssets<Sprite>("wizard_clues");
            wizardTowerLensClueSprite = wizardClues[0];
            wizardTowerSubmergeClueSprite = wizardClues[1];

            boonTableTex = assetBundle.LoadAsset<Texture2D>("BoonTableEdited");
            smallClockClueTexs = new Texture2D[12];
            for (int i = 0; i < 12; i++)
            {
                smallClockClueTexs[i] = assetBundle.LoadAsset<Texture2D>("SmallClockClue_" + i.ToString());
            }
            factoryClockClueTexs = new Texture2D[12];
            for (int i = 0; i < 12; i++)
            {
                factoryClockClueTexs[i] = assetBundle.LoadAsset<Texture2D>("FactoryClockClue_" + i.ToString());
            }

            cardPackPrefab = ResourceBank.Get<GameObject>("prefabs/cards/specificcardmodels/CardPack");
            selectableCardPrefab = ResourceBank.Get<GameObject>("prefabs/cards/SelectableCard");
            selectableDiskCardPrefab = ResourceBank.Get<GameObject>("prefabs/cards/SelectableCard_Part3");
            cardChoiceHoloNodePrefab = ResourceBank.Get<GameObject>("prefabs/map/mapnodespart3/CardChoiceNode3D");

            clockCluesPrefab = assetBundle.LoadAsset<GameObject>("ClockHeadClues");
            archipelagoUIPrefab = assetBundle.LoadAsset<GameObject>("ArchipelagoUI");
            smallClockCluePrefab = assetBundle.LoadAsset<GameObject>("SmallClockClue");
            gbcSafeCluePrefab = assetBundle.LoadAsset<GameObject>("GBCSafeClue");

            saveEntryPrefab = assetBundle.LoadAsset<GameObject>("SaveFileEntry");

            checkCardHoloNodeMesh = assetBundle.LoadAsset<Mesh>("CheckCard_mesh");

            // I have no idea how to edit an asset bundle, so it's time for some base64 stuff instead

            menuCardAct1Locked = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAABQElEQVRoge2avQqDMBCAT8nk5uTSQehDWHwpn6UvJfUhCg5dnNycxHYokbP4F3NBr94HhdaG9uPOO5IYzw/CNzBAAQBkabyzxjz3vPyK6g9HRAdR4Yu362UPl0kez1f/3t/RwwgRpaRrah6ifhDyEAWQ1NMiqXeBiFLDRlQtD5knidb9RFG1Vv9jJaoll6aIWRpDEikr2c2pXyuJx6yN/i9dU9tFFEtOrRL0mHtebl5JkDV8LVBU7eCFv7OFrOrH7j/bAsKwaU8iSg0bUeP2hHshrui5Hjk2zrTQNvVR282KLS2LTerJRbM0drKXdd6IusJ6PgowXhz4GsVO4bkiOjbdo95vZRNRNqIkqce42mL/74ju8RTFWJRyeWECm9SLKDUiSo2IUiOi1IgoNWxEB5MSfOLgaPSiRz8A43E5TvQBWfRYzo2byp4AAAAASUVORK5CYII="));
            menuCardAct1NewRun = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAACFElEQVRoge2ar1LDQBDGr51MRV1NFIIZTH0MwyvkUSp4FkQepa/AYOJjOoNA1eAQEYBgtmzSu739B20Kn2rvrtdfvtts77aZzZerjzABFSGEcH93fWIMWg+Pz1+g8OYcBSYWuPH25uoULEk97V4OrwtinEjNuo+2b7qFy/wuoABZbfeD9rYuQ7PuXWDn1glSkLgt5TZX72+vdlAMJO3jar5c+YD+hiYB6rb0Py23pW/rUtUnkRkUUk8MCNo80pM5j+LUk3LPI5eqQMd5MZeCPBK/GJRK8ClV2/0BNgRdKLBjtFn3KkhQtd2bfqlYoBjQ+kuDYSXAJKjGxbYusylJ6i6Z8D1dTInrbjThW2NRKq67g7teC0gle+58ucxwlJ44OVECIBHMB8DV7ruPlUfHjm26xWCZMDCMjV0E9yLBXSwSFA/2OvvARUpX5gg05p638JwYmgIegHpASeMWvnPs8lhux2WrciZNYocfwj+ovy4b1FL50H5W7ah286zVZS/9KSQGtVbmtHOoHKViLXcU0cbp5S49JeykV80JJALlxNamW7B2YdI4FTuairHcuYkzByXXpcdOem+4/059FBRz0NNVUZEsJo5jVN2UK5GjqZuAci7VJ72hTEsviT9rrLrV8K1jclKDahyyuCoCtRQnxmOl0OxzfawUo5G2TCQqQFhjze1fEfzEwbnpAHruD8DMpvI40SeunwwIJqgVWAAAAABJRU5ErkJggg=="));
            menuCardAct1Continue = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAADCklEQVRoge2av2rbUBTGj43I4E5eoiWDIUvGQgcFQ5+g79CtUwZD36RDX8CPEOgThBRrKGTMYsjgRV28BRpE28Ec5ejc79w/kkIi8LdY1v33u9+591xLeDKdzf/RCJQREa2Wi1fG8Ovbz4cDKH95i2ITp6+LEa8j6NAaBejfx/04QKez+ThAiUYU+ixczdavT6fN9YcfvzvXCalX6BmAB5dAKXVi1RlUD47ciqkTq16hjxm4D5zUKDYT0RF0eB1Bh9TxrH8JjQa0V8K3Hgpf4vkrCTT2aVXXGwI8CBqC0xCovrzXFTo59Hqgy/Mzb/lQ7wyCoCEwrRB4V0U5GoKLbbvZ7jr30wJFQFbnRZ7R5/kjLFvvZ1RWdRSAzwQ5dqYLfA2L3A3A+4/td2x3NxOnroT2ZQSf+46jCBYBEh2cW1/DIth+s3XLdEaQDBLWOZl0Be6AZ56yOWQbfe3rBzE4ViFH5axT0g3X5U92jGVlFOSoA4rCz2usyDMqq5q+Xzw1a/PuZkJX9ydERHT75U9zj4jo6v6kaYOE1qS1T+CPEmS9BE5RahtrbLhL9Ky4YdPJxWnjmtTX63eqnwdnYGtj8kazHIWtdPhlw8P1ExxMu6fbb7a7oMNWejRPJl8+Xe9n6k58eK0DZLVcOJtNKin0z9+juRxpkM1212QFK49DUG7Is0ONrHVmhdV34l2enzmJHqm162XF1XIBwyTTTVnVrWs0AZ64G5Vdq7zIM3PHO6Bcsaxqc71YQChforBqeHmA+GCho0WetVzQ6QWFGE2Aw1pWdeu0kfDydJL1vKAMyZKzZlgJVORZ61rDorBq+M6OSrdk+BlWOyrXqSzzQUh49OOks6NyzbJrofBbYUXw7LDvMPC+gGA4vWZDG6rIMy+Uho951HFAeWYSkoXCL9ephGX3pVLWZBCUJVMVS4dfT46vUZkFz2N1BpWpimVtKCm0+7UkfKy8jqIBQhtKwg6p4HO9NaDOp6hsSNjOrx1j8qkv/Klynutj9fyLHN9HZX3UgL71P8BMxvJ3ov+Y+XfSZjhqggAAAABJRU5ErkJggg=="));
            menuCardAct1Complete = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAACp0lEQVRoge2asWobQRCGfx8qglIJVDrkICoMdpNKJkK4daP4BdIGVBjiKu9hcGEQ6vwCkZ4gGEJUqUmRJqCg9K5iUikpzB7rvZnZ3ds5WQf5m9Pt7c18O7M7e3doL2t3/qIBagHAhzf5E2PIuvyyegA1J7soE8TsaTHC9R9UWy1/F78m4yHmixV5bdTP8f76Nsn+5v5OB3S+WGHUzwEA2bPnGPS6uP32s7iWqqzd0QEFgOHRS3ycfgYAfHo4YLZc4+3rFyr21ebowfkNAODs5BBnJ4cAgOnFqYrtzf2dHuj3q3cAgEGvi0GvW/zWkGrqD85vML04LdJv2nYu9ZPxEJs/v4vzUT/HZDzUMq8XUSOz+gGdFW+kEtFRP8d8sSqB2dCpUq+jFOxsuU72oQI6W65ZGA1IoEF7fSNAVQt+ncranWaAAg1JPZC46o9f7ZPtX3/8SjFLKgqUA/P10wD3gvrgXAiqv91WFTo69T6wEPAq8oLGOq4j7UBgRFOiopF2wAGlgCTj3LWYgUl9bfst94LWnOIkTQ0p+qWIxsBqDMqFsxls2NLO5HYwBkxbzDyz73F/S3YohtJioiJqn1eZf+bo2uYqChXREmhI+iUHVLtvIPa9nG/yoYQK/bbE+SZBqRRpzdGQe6iIkgXfTb87R2PrJzc1YmywO1Nd9VQapLQ2SFDf6gx1zgFxviRYtjyZo0ZkpUi5BZ7r92gxUaPzAYT04TaRGJ+PQO2OGvs+ByFFMKg8SYaryAfHRdpb8LnRcaP0zWFuoUhZC46ozxEFw7VLA5dsVo4o51hSaFpDt2vxA0RsFagCFRoAFjTUETdPudca9YjGjD70ocP3LCApOaLbkhjRmPa6pfJpfBvwap8dYx6Oq6j0Xp+qumAL0F3/A8xeU/5O9A8lh9RKc84fxwAAAABJRU5ErkJggg=="));
            menuCardAct2Locked = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAABRElEQVRoge2aPQ6DMAxGDWJi4xbtDViRWLgK4jiIq7BU6tobtLdgY6QdKmiK+EvyReDWbyrIEk82MUkazw+jJzEgICLKi2xvj0Wqsn6L9hdHpE9ioN48n6JdZOa4P5rht7+jhxYiiqRrGx6ifhjxECWS0mOR0rtARNGwEQ3WQ5ZJ0nhT3PVys3qOlWgvuTZFzIuMkjS2kjUu/VZJNWZr9sd0bWOXUVVybpXQx1RlbbyS8MPI/h0l+kiOS5ukMeVFBlk9wEb91PtnO4BU2LQnEUXDRlR71Ku9UG03Sz1yKk53oBm1J9t2Y9JP2ZQeLpoXmZO9rP/NqCug3/q5e4f61rsGktGp6R56v5VNRtmIQkqv4mqL/bczuse/KNqiyOWFDmxKL6JoRBSNiKIRUTQiioaN6NekRD1xcDQG0aMfgPG4HCd6AU43VTxO+reFAAAAAElFTkSuQmCC"));
            menuCardAct2Start = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAAB+UlEQVRoge2aTW6DMBCFHcQqu9wCiZNwlYI4DIJchZMgcYvuumy7qIwGYo/nLyXQvlUCjvl4Hsb2hEt2vX25Ayh3zrlp7vbmQFUW7Q+o//KK8ibm8OBbU+0CE9O9H5fPOdKOpTpykwO4mEYmoB5yGz7T3Lm6qUxgM20HMUh4LOY2VZ8f73pQCMQ9R1V2vdmA/oYOAWo29M+W2dBjM5vVrKcG9aknBOSPWaQndR6FqSfmnkUuFYFu82IqBVkkfjYoluBjKot2gXVOFgrkGK2bSgTpVRataqYigUJA7UwDYTnAKKjExWnukimJ6y6a8C1djInqbjDha2ORK6q7q6deCogle2p/qczw4CglJ1LiUCLMXVIe3UIN/bjqCN6cbxu6YarL3l0yKGxstffxN8kNjQfQkHvWgn1CaAx4BWoBxX0Q/TW3Lm9ltl3WKmXSIVb4zv2D2uvcoJrKh/S3Ykeli2epzj30e4gNqq3MSfsQOYrFWmoJKI3T8w49Juik9cKaBUqJraEfSaswbpyyHY3FWGrfROkDk+nQQyetF9x/pz7qFXLQ0lVWkSwkimNY3ZQqlqOxhwBzLnaO+0Cphp4Tf9pYNavha9ukJAaVOKRxlQWqKU5s23Khyfv6UClGImmZiFWA0Maa2b8i9yfUmay0gL76CzCXo7xO9A1hWAX7kcujpAAAAABJRU5ErkJggg=="));
            menuCardAct2Continue = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAACQklEQVRoge2avU7DMBDHL1WmbhEzQ7e04RWQoBuPAUNFVZG9T8AeVBV1oDsvwEYr8Qo1ZEOoc9UNiQUYKhfHOdtxcmka0f+Sj7OdX+7DTqI4jab3AzWQCwDA4qhqDq0CP9yA8oN9FHeiK568e+pXAqPSzcV4u9+okMNKrrlJdnVb7cTx8/sb2dhkHpUhVefy6PtzTQOqA6KAbTS9+uRoLUDJQl+2yEKvq26qyifzKAZEOT2RzqOUYLJqkaMAOwTtttqF5tRCoecXVoVctVrlSZFcHpW9g3mLerUygmJQlABZpJ3wbQB1/XTtsC0m6wmf2mNWsLsAspHq2m6WRrsWxpEALXNlKaoE6OPHV1UcRiVAX9ebbxEdz4GX6we0w+n9FQBAqXbOoQTlkFzyh4nADxMXwOyi8to7npOCTYGKDeazhWzeDh74IWoXlceOeRMFXQ2n2sFNUoU0qx3zJoAEuhpOEx5jbKkdFLOb+pvsmTyaypngWAtqYx/0J3+AHBbpzyN6dHuZOI+uTDypB/0JMLbUetbGjn01lPuzOAIWR6kUVD6PsjiCwA9JvWpj59fnMj44n52fJDpj+1ns/FjeqvrLQkFZHMF8toDRuGecYqglOkZUbV7ucr8z8SoGABiNeyQwOhV6uZMTHhPVDSlBVbmSpy2/IZsxZeX2qKp6xeIrAiar9GLSTUc2Iv32BEDrRVGkoGWFHeA/zKOYygo7QAk5Wtuqp9Kh6g9VT6FD1UNdQ7+vv2sACKD7/gOMU5ffiX4BjNoQjWzZNSIAAAAASUVORK5CYII="));
            menuCardAct2Complete = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAADrklEQVRoge2aTUsbURSGX1MjEkGUQHCQkChliIztsnXVaqlCCnbXnZBN3bq1P6FdBdzajdBf0ICFJKjtqu2mMHUwBKmKSGRgqJTGhkprF3qmd+7c+YozVasvhHyc+54899yPzB3SEUv0H+MSqBMAtNrCeXO4SsnNnYDSm4soKmIn++GNRPJcYJz069AwX8fOkSOQOr2b+Fcl8whD0iC2Gnu4O3UfvYvzoeUOraKbY7Mm5JA0iA/lt/g2+yKU3L8Pv4YDujk2CwAmJAAMSYPQP2+GAhtL9Ic7R6miUShUUKroVmMvVODQhp6g2DkapnwPfV9qwDU2ubNsqSLBpm7dRO/ivKffF6wfyAN93zF+oO+bsCQe0o/fS577aEuW0HeaUKS+1ABasgTo+39hdwC8D+53k2dFu+uNk0QOaskSuuuNyPwkT9CWLKEgxx3jBTnuCVKQ46hk8qhk8oH9vkELchxL9SNhrJLJY6l+5NiRSiZv+ouGiqKh2mDd/IFAFSVtS0TVKRoqCnIcipK2AVCc9/Ow5D8zqKbtWhIRQNFQzY5o2q4NgOK8nxf53eS64VPVFCWN8YlR4fwCgPGJUROEYNm25GfFdor8TvkBHxt+0VAxPjGKtdV1MzlVirS2um4BYatNIGur68LclUze9IvmrwVW9CELxILwkG4gTh3hYVm/G6wFlF0kLIiX3NoE9YumD8D9Mk3uvLEleX76vPxdfDUURVzUtiOW6D/WagtQcnOYTt22Jnn31PL+2b2X/zRe0lUQm6WiX/ATADCMLpR0FSXBEZo6E2WcOFjZLkqG0WW+XhmZscQebLxCSVdd46zajQ+jywZrA2UbTKe5K5oNIJlVAADGtiaMs2onLqqmEFTTa8KGfsVWvJ24qJoAB6rpNUvF+MuvJmcWxb38XnFfFaUkJNvl17bWdrxZrpqA9D0iP42okso5g5KM0y9slqvmTao7c69FTfFx4bGvuJKrYmVkxrageD91QuM67XgUoaROAKQg8Sc/Pvn2J7OKWTBXUErKzqNuZmokswrAzTG3OMWS7LOLn5cQNJlV0JIl9MgSWo7WaOR0fvr/bzvSKgaAnqmHocC46UwV5X8CRWqWq+bjLHKsqNdZuwnxghO1ox3Ez/ndSW0PfZJbxSR2Eycw0bYUVJEvJqcOBVWo9/AB7ynTrkIFFQ17WPr/91GRohp2III5GtXwX82hB65X/RUd+utVj8s69AZ38mtLTA7+PsBZZIJe9D/AdFyWvxP9ASZJET2yrNfdAAAAAElFTkSuQmCC"));
            menuCardAct3Locked = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAABSElEQVRoge2aPQ6CQBBGB0JFxwFMjAl3oKHhFHScjY5T0Nh4BxNj4gHoKNHCgCvhb9lvA6PzKiGT8DLDDrvrOq4fPIkBHhFRkqVbe0xS5sVbtL3YI20SPfXm4XTcwmWUx+3e/Xa309BDRJE0dcVD1PUDHqJEUnosUnobiCgaNqLefMg0YRwtirueL0bPMRJtJeemiEmWUhhHRrKrS79UUo1Zmv0+TV2ZZVSVHFsltDFlXqxeSbh+YP6OEn0k+6UN44iSLIWsHmCjfuj9Mx1AKmzak4iiYSOqPerVXqi2m6keORSnO9BWtSfTdrOmn7IpPVw0yVIre1n/m1FbQL/1Y/d29a23DSSjQ9M99H4rm4yyEYWUXsXWFvtvZ3SLf1G0RZHLCx3YlF5E0YgoGhFFI6JoRBQNG9GvSYl64mBvdKJ7PwDjcDlO9AKhK1SH4rq7qgAAAABJRU5ErkJggg=="));
            menuCardAct3Start = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAACDklEQVRoge2aPZKCQBCFR8pIIw7BRTyA8WabE3EBIi5ARG5mzAG8CIfYiA3XDbaGakam6T9XYPdVWaWIw8frnmZs2CWH9O5WoL1zzrV992oOVOdj9gPqPyxR3sQ93Hh6f3sJTEy3y3V4v0f2Y6loqsntdV6ajJ9YDOIhz8ds9ILfaaUGhZChrGC/Pj9sHMUmosUkTQ6pDehvaBWgZqF/tsxCj13ZrK56alBfJ6eA/DaLWqou+LD0xNwrmkoNKwIN6+JcCWr7Tg3LBsUKfEznYzbAOidLBXKOFk0lgvTSXlZJoOG1XCMIywFGQSUutn03W5K47qIF39LFmKjuThZ8bS5yRXV3lxzSe9t3w8yUAFJCzR2nzkt3u1ydZ3twlFITKXkoEeYuqY6GUHVejgaCJ4dFBY4zt4YNj4mChqGwkD9JKrTXA+iUe9aCY0JoDHgEagHFnYj+mKHLocz+Lms1Z9IqVvjO/YPaa9ugms6H9LdiR6WLZ6m2HfpXiA1q0UZ82n+mUFiuzS0BpXm63dBjgk5aL6xZoJTcqvOStArj5inb0ViOYU0y6hiYTEMPnbRecP+d/qjXlIOWrrKaZFOiOIb1TaliORqbBJhzse+4E0oVek7+aXPVrIev3WdOYlCJQxpXWaCa5kS4LxeaDApv02icgb/nnCirAaHNNbO7IvCJg6VpAF36AzC7tTxO9A3xjRDDiAAWEgAAAABJRU5ErkJggg=="));
            menuCardAct3Continue = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAAC80lEQVRoge1avW4aQRAeTq5wBYYSCWFRuIioQG7cRErrOl0eIM/jB3Dn2q0lN2kiqJCLFMgIidJJXNmlnSIaNLc7OzszHDmQ8kmn5bjd2e/mb2cXGkWz9Q4HgCMAgNuXRd08RFweD/8SxZt9BCrxiH7ZG/Tr4JLEernafC7qo2HDf6JV4u31+TCIFs1WOZgkoGP3Bn1YL1el1jI+hHa8migAwGgyhvl0FrXayUaTcfSdZvzb67ON6Hw6Y9uUtlLjKTQvqzb9ermCj18+q8hYcX99o+qXDaaqSCKhkJhWtsn02rfPjcfWogATUQA+IDzg/FWCO4/+/vmUbKVnXriIaohUTbb2lenXj0dVP7WPcgKvHu5K918/fDI910JM+GESPzk7BQgCIDexl1iIotmSTU/Th9ZEu0KJ6Hq52lwhTs5OzcJD02+DyPSoRW9yt5LjXIpDRJQS5Mhi1cSlmauHu41f0s8U7U7XnOwjolwFQ92AapsjS4lJJD21gymP0jV6Pp1Bu9M1XUgyVaBIUOdRKhw14jGhF2qiqAkkuav6NIVG0Wy9374s4PJ4mKyyd104S/MiN5VGe4P+1rWoJFsDtelzAjmtS9qywrxdlsBpPTfOpVEqlAqw+Kh1m6HVeokoFU61YwmkVN/Ft+8wvDiPWswmSDZlAdH0uOeuAsOLc7ZFIMGUsmqv8BG5TePeEM0hS3Q0GZty6P31jbk/QH45Fn3U6p80KOhymwON+tRLihrtDfqbK6elMM14xoRzUpQ0msuB0sRcMLQ73cqW3hJRKR3RPEeL5nanCwDpgwV8AdrfcqbKEkVoVgskSO9DsrRP2N+KiKgmyWuDTHvAq4E5j4bOjqbFe/qM68MFyk6IhuDMHZp5m1M8xNZEOVKSr3oREbX6X/iDAz1pSfXxoBRMKDAsvTiS3j2Ut+pP1qMpVBHJlh/SEC4ftRYqCDxh8aBE1Fr5/EtEZ0+azRj6suelPMsnQHAAsY+IDiD2/Q8wjUP5O9Efh4zSzyLrPXYAAAAASUVORK5CYII="));
            menuCardAct3Complete = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAACLElEQVRoge2aS46CQBCGC+NCnYUhQth0woorEE4xh51TEK7A1o1RM3Exo6thFqZI2/T7oZL0v0FbQn/5q6uqAZPFJh1gBloCAHz99K/mkOrzo7qD4pd3FJq4pAeH1e4lMCIlt/P4efFCDiNFUJ/6+/2eB+hik84DFCCG3q9i6EMogvpWBPWtCOpTseCH0FNAk9v5Ybduo6X6FHshXFYQAAA4HfbWtzvBQJPbeQQEuEPiuA1sEFAaEgGrugEAgL5rra4ZBHRY7SaAqKpuoO9aY1e9g+K6ZAFd5Q1UBojhxt9sXDUGRSCcRAcQ12vftdZOa4PySo0uIE9ZQYzKlRLUFfB4uUK+Xd+PBbF2VQoqKzO6gCKlpVkTkILKygwP8Hi5AgBMAHHcxVWlozYhpsFkSksCaUm0KoAQlAepCrEKbDzfwlXt3VPftZAVZBJmXUBUvl3D6bB/aKlV3Sh3V0LQYbVT9mVVwsggTXu+tqNV3YyJZRpu1XV1XJWCyly1Bcyo9cm2VpmMWih2k7wgxusSYFqLaUBV5itB0dWqbiAtyTiZCyAtdNW6PKHYdUO7imLd9QmoBUrXUp6reMSlEAJQC1QkOlNx8rwgQQBR2lnP1j42a7E+8roZJorLCzdl6EVCSHRW1G59vQ3U6vX0pDQYr1/7BlSC0mEXTcrWQdm5rlLuR3ljbCKJzvUpq6zXcdu3rG+Xn/3KPD529K2H0Ls+wwypEfTd/wCTzOXvRP9Jj4YMxVtpbQAAAABJRU5ErkJggg=="));
            menuCardAct4 = GenerateSprite(TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAACoAAAA4CAYAAACL3WrLAAAACXBIWXMAAAsTAAALEwEAmpwYAAABWklEQVRoge2au4rCQBSGjyHCYqXPIVhuuT6AVUAs3MZWS5t9BhvL3TaNFiJY+QCm3HJhn0MrEQR3C0kY3Z3xnMkJkwnnq3KZy8f8k0wCUwsarR/wgBAAoDkduvYwcpgvr6LpSRlJBzFULz51np3I6Dh9fWbHgUMPEiLKyeW490M0aLT8EAWQ6HmR6ItARLnxRjR8XARgN+tp73XftqiO1DawdVRQogAASRT/ufayGZE6S9vYbUZkWbSoDtNo60iimCR7Oe7zi06Ga1S592X/5pwi6/yFn0QxOpFqPfUm7iMtilyi/70JiqLa0X/X67k7bp/PpPLkEXUhCWAxojadcCDRm5Do75HoDVQ3eurHsg7q8ms1Rz8GC5tqGePVK7mON2u9iHKDnqPqQ2Qzx0ztYUCJ2vyHc+NN9CLKjYhyI6LciCg3IsqNN6I3HyXqjoOykYmWfQNMzZftRL+Wil8BwUvNRwAAAABJRU5ErkJggg=="));

            lockedNodeFrames = [
                TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAADEAAAAxCAYAAABznEEcAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAt0lEQVRoge3Y3Q5FMBhE0fL+78yVxE9EzQwfyV732pmjaE9rAAAArxjC400Vc6cGuhM+niFRwimwsHKM5uSJAjbnF0gXkLM8VeJs3KviUh61xFmY3vHc6zfcZ8IOsCMtUaVE+dtoL3UnlFDqc3OQXE5lqktEllV1iQhKfAUlVtRvR2T/lbwTdwPFth7pvVPvuMrmMXdBRwjHqxvA9Nnc8pXjaWtFh6I1t4iVo/IvmyfmBwAAAH5iBg0aFiXSGoaoAAAAAElFTkSuQmCC"),
                TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAADEAAAAxCAYAAABznEEcAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAtklEQVRoge3Y2xZAIBCF4Vje/5W5Yrkg2e1J+L97055yKCkBAAA0MQTUnFuP6ypWEjxsfEcRtYG9qhy1TTgaWMlZRmOIWvKETEGD5mbVuXqXg105C1Nas/b6jXo7OQLYXrM9PRMyZTZst0Gm3q1an1gJVxMR25dirEQvaMLA8vV2NaGEsW0/nCtxJ5S67/JcUBAiVzfk1BfVhErKE7GLVclZejnZPXo8Xb3+R8ER9cEHAAAAfmIBIIAYI4UExqIAAAAASUVORK5CYII="),
                TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAADEAAAAxCAYAAABznEEcAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAxUlEQVRoge2Yyw6DMAwETdX//+X2VIki2vixCUaaOQdnNybGxgwAAABgCduEmK/VeytNeMXLNShMZMXvKemomlAY+JDW8hCKqJI+kEomfm06ijkSG9akzoRHwOZc5yZrYuZlDsdWZmLGN8dFxsSKihTaQ5WJy7Jg1qPElg+gg4kymOhCBxPlaqcyoSy7YTImOswgXyhfp6igf+tDBzWjix3Flnexz+gDAZbdk06TnVlSDzP2gVv/7TjDY+jSzhcAAAAAbswbQzwXKg/q8kQAAAAASUVORK5CYII="),
                TextureFromBase64("iVBORw0KGgoAAAANSUhEUgAAADEAAAAxCAYAAABznEEcAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAwElEQVRoge2Yyw7CMAwEDeL/fxlORhwgkTfrtBUz18prb2zl0QgAAACALdyadJ87c7oFR8W35XYJVYq31+AwsWIgWapj1YTDQCLXsmJiZuCbthIz5aEETRgVkt+cHZQ78auIip5DIyIi7tWAAdXkti1WMWEdBYe+qxPqqlq64Rynw8DEWcDEB+qOZdnpFBNdbxBZ3zlO1VW1nTe7L4CzuNNcAJPuk/3N378nksu/7BLViCX/Eb9sOvMCAAAAAFyEF9GHFymRK1e8AAAAAElFTkSuQmCC")
            ];
        }

        private static Texture2D TextureFromBase64(string data)
        {
            var bytes = System.Convert.FromBase64String(data);
            var tex = new Texture2D(1, 1);
            if (!ImageConversion.LoadImage(tex, bytes))
            {
                ArchipelagoModPlugin.Log.LogError("Could not load a base64 image!");
            }
            tex.filterMode = FilterMode.Point;
            return tex;
        }

        private static Sprite GenerateSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }
}

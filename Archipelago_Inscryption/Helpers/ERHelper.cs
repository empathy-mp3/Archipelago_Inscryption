using Archipelago_Inscryption.Archipelago;
using Archipelago_Inscryption.Assets;
using Archipelago_Inscryption.Components;
using Archipelago_Inscryption.Utils;
using GBC;
using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static GBC.DialogueSpeaker;

namespace Archipelago_Inscryption.Helpers {
    internal static class ERHelper
    {
        private static readonly Dictionary<string, ERTransition> gbcObjectEntrancePair = new Dictionary<string, ERTransition>()
        {               
            { "GBC_Starting_Island/Room/ReturnToMapVolume",                                             ERTransition.StartingIslandExit},
            { "GBC_WorldMap/????",                                                                      ERTransition.WorldMapStartingIsland},
            { "GBC_WorldMap/????",                                                                      ERTransition.WorldMapDocks},
            { "GBC_Docks/Room/ReturnToMapVolume",                                                       ERTransition.DocksExit},
            { "GBC_WorldMap/????",                                                                      ERTransition.WorldMapMycologistsHut},
            { "GBC_Mycologist_Hut/Room/ReturnToMapVolume",                                              ERTransition.MycologistsHutExit},
            { "GBC_Temple_Nature/Temple/OutdoorsCentral/ReturnToMapVolume",                             ERTransition.WorldMapForest},
            { "GBC_WorldMap/????",                                                                      ERTransition.ForestExit},
            { "GBC_Temple_Nature/Temple/OutdoorsCentral/Floor/ChangeRoomVolume_Pond",                   ERTransition.ForestPastProspector},
            { "GBC_Temple_Nature/Temple/Pond/Floor/ChangeRoomVolume",                                   ERTransition.PondExit},
            { "GBC_Temple_Nature/Temple/Pond/ChangeRoomVolume",                                         ERTransition.PondHiddenPath},
            { "GBC_Temple_Nature/Temple/PondSecret/ChangeRoomVolume",                                   ERTransition.PondOldDataRoomExit},
            { "GBC_Temple_Nature/Temple/OutdoorsCentral/Floor/ChangeRoomVolume_Meadow",                 ERTransition.ForestPastAngler},
            { "GBC_Temple_Nature/Temple/Meadow/Floor/ChangeRoomVolume",                                 ERTransition.MeadowExit},
            { "GBC_Temple_Nature/Temple/Meadow/ChangeRoomVolume",                                       ERTransition.MeadowHiddenPath},
            { "GBC_Temple_Nature/Temple/Woodcarver/ChangeRoomVolume",                                   ERTransition.WoodcarverRoomExit},
            { "GBC_Temple_Nature/Temple/OutdoorsCentral/Cabin/ChangeRoomVolume",                        ERTransition.ForestCabinEntrance},
            { "GBC_Temple_Nature/Temple/Cabin/Floor/ChangeRoomVolume_Outdoors",                         ERTransition.CabinExit},
            { "GBC_Temple_Nature/Temple/Cabin/Floor/ChangeRoomVolume",                                  ERTransition.CabinShopEntrance},
            { "GBC_Temple_Nature/Temple/Shop/Floor/ChangeRoomVolume",                                   ERTransition.CabinShopExit},
            { "GBC_WorldMap/????",                                                                      ERTransition.WorldMapCrypt},
            { "GBC_Temple_Undead/Temple/MainRoom/Floor/ReturnToMapVolume",                              ERTransition.CryptExit},
            { "GBC_Temple_Undead/Temple/MainRoom/Floor/ChangeRoomVolume",                               ERTransition.CryptShopEntrance},
            { "GBC_Temple_Undead/Temple/ShopRoom/Floor/ChangeRoomVolume",                               ERTransition.CryptShopExit},
            { "GBC_Temple_Undead/Temple/MainRoom/Floor/ChangeRoomVolume",                               ERTransition.CryptMirrorRoomEntrance},
            { "GBC_Temple_Undead/Temple/MirrorRoom/Floor/ChangeRoomVolume",                             ERTransition.CryptMirrorRoomExit},
            { "GBC_Temple_Undead/Temple/MainRoom/StaircaseVolume",                                      ERTransition.CryptBasementEntrance},
            { "GBC_Temple_Undead/Temple/BasementRoom/StaircaseUpVolume",                                ERTransition.CryptBasementExit},
            { "GBC_Temple_Undead/Temple/BasementRoom/StaircaseVolume",                                  ERTransition.CryptBasementBoneLordStairs},
            { "GBC_Temple_Undead/Temple/BonelordRoom_Stairs/StaircaseDownVolume",                       ERTransition.BoneLordStairsDown},
            { "GBC_Temple_Undead/Temple/BonelordRoom_Stairs/StaircaseUpVolume",                         ERTransition.BoneLordStairsUp},
            { "GBC_Temple_Undead/Temple/BonelordRoom_1/StaircaseUpVolume",                              ERTransition.LowerBoneLordRoomExitStairs},
            { "GBC_Temple_Undead/Temple/BonelordRoom_1/ChangeRoomVolume",                               ERTransition.LowerBoneLordRoomToUpper},
            { "GBC_Temple_Undead/Temple/BonelordRoom_2/ChangeRoomVolume",                               ERTransition.UpperBoneLordRoomToLower},
            { "GBC_WorldMap/????",                                                                      ERTransition.WorldMapFactory},
            { "GBC_Temple_Tech/Temple/--- MainRoom ---/Floor/ReturnToMapVolume",                        ERTransition.FactoryExit},
            { "GBC_Temple_Tech/Temple/--- MainRoom ---/Floor/ChangeRoomVolume_Shop",                    ERTransition.FactoryShopEntrance},
            { "GBC_Temple_Tech/Temple/--- Shop ---/Floor/ChangeRoomVolume",                             ERTransition.FactoryShopExit},
            { "GBC_Temple_Tech/Temple/--- MainRoom ---/Floor/ChangeRoomVolume",                         ERTransition.FactoryToInspectionRoom},
            { "GBC_Temple_Tech/Temple/--- AssemblyRoom ---/Floor/ChangeRoomVolume",                     ERTransition.InspectionRoomExit},
            { "GBC_Temple_Tech/Temple/--- AssemblyRoom ---/Floor/ChangeRoomVolume",                     ERTransition.InspectionRoomToSmeltingRoom},
            { "GBC_Temple_Tech/Temple/--- SmelterRoom ---/Floor/ChangeRoomVolume",                      ERTransition.SmeltingRoomExit},
            { "GBC_Temple_Tech/Temple/--- SmelterRoom ---/Objects/Elevator/HoleVolume/DropEndPoint",    ERTransition.SmeltingRoomElevatorDown},
            { "GBC_Temple_Tech/Temple/--- DredgingRoom ---/Objects/Elevator/HoleVolume/DropStartPoint", ERTransition.DredgingRoomElevatorUp},
            { "GBC_WorldMap/????",                                                                      ERTransition.WorldMapTower},
            { "GBC_Temple_Wizard/Temple/Floor_1/Floor/ReturnToMapVolume",                               ERTransition.Tower1stFloorExit},
            { "GBC_Temple_Wizard/Temple/Floor_1/Floor/ChangeRoomVolume_Shop/ChangeRoomVolume_Shop",     ERTransition.Tower1stFloorShopEntrance},
            { "GBC_Temple_Wizard/Temple/ShopRoom/Floor/ChangeRoomVolume",                               ERTransition.TowerShopExit},
            { "GBC_Temple_Wizard/Temple/Floor_1/ChangeRoomVolume",                                      ERTransition.Tower1stFloorBackRoomLeftEntrance},
            { "GBC_Temple_Wizard/Temple/BackRoom_1/ChangeRoomVolume",                                   ERTransition.TowerOldDataRoomLeftExit},
            { "GBC_Temple_Wizard/Temple/Floor_1/ChangeRoomVolume(1)",                                   ERTransition.Tower1stFloorBackRoomRightEntrance},
            { "GBC_Temple_Wizard/Temple/BackRoom_1/ChangeRoomVolume(1)",                                ERTransition.TowerOldDataRoomRightExit},
            { "GBC_Temple_Wizard/Temple/Floor_1/Objects/GemPedestal",                                   ERTransition.Tower1stFloorGreenGem},
            { "GBC_Temple_Wizard/Temple/Plane_1/ReturnToRoom",                                          ERTransition.GoobertsPlaneExit},
            { "GBC_Temple_Wizard/Temple/Floor_1/StaircaseUpVolume",                                     ERTransition.Tower1stFloorStairsUp},
            { "GBC_Temple_Wizard/Temple/Floor_2/StaircaseDownVolume",                                   ERTransition.Tower2ndFloorStairsDown},
            { "GBC_Temple_Wizard/Temple/Floor_2/HoleVolume",                                            ERTransition.Tower2ndFloorDropDown},
            { "GBC_Temple_Wizard/Temple/Floor_2/Objects/GemPedestal",                                   ERTransition.Tower2ndFloorOrangeGem},
            { "GBC_Temple_Wizard/Temple/Plane_2/ReturnToRoom",                                          ERTransition.PikeMagesPlaneExit},
            { "GBC_Temple_Wizard/Temple/Floor_2/StaircaseUpVolume",                                     ERTransition.Tower2ndFloorStairsUp},
            { "GBC_Temple_Wizard/Temple/Floor_3/StaircaseDownVolume",                                   ERTransition.Tower3rdFloorStairsDown},
            { "GBC_Temple_Wizard/Temple/Floor_3/HoleVolume",                                            ERTransition.Tower3rdFloorDropDown},
            { "GBC_Temple_Wizard/Temple/Floor_3/ChangeRoomVolume",                                      ERTransition.Tower3rdFloorBackRoomLeftEntrance},
            { "GBC_Temple_Wizard/Temple/BackRoom_3/ChangeRoomVolume",                                   ERTransition.TowerDarkRoomLeftExit},
            { "GBC_Temple_Wizard/Temple/Floor_3/ChangeRoomVolume(1)",                                   ERTransition.Tower3rdFloorBackRoomRightEntrance},
            { "GBC_Temple_Wizard/Temple/BackRoom_3/ChangeRoomVolume(1)",                                ERTransition.TowerDarkRoomRightExit},
            { "GBC_Temple_Wizard/Temple/Floor_3/Objects/GemPedestal",                                   ERTransition.Tower3rdFloorBlueGem},
            { "GBC_Temple_Wizard/Temple/Plane_3/ReturnToRoom",                                          ERTransition.LonelyWizardsPlaneExit},
            { "GBC_Temple_Wizard/Temple/Floor_3/StaircaseUpVolume",                                     ERTransition.Tower3rdFloorStairsUp},
            { "GBC_Temple_Wizard/Temple/Floor_4/StaircaseDownVolume",                                   ERTransition.Tower4thFloorStairsDown},
            { "GBC_Temple_Wizard/Temple/Floor_4/Floor/ChangeRoomVolume_Bathroom",                       ERTransition.Tower4thFloorBathroomEntrance},
            { "GBC_Temple_Wizard/Temple/BathRoom/Floor/ChangeRoomVolume",                               ERTransition.TowerBathroomExit}
        };
    }
}   
# scene names

GBC_WorldMap
GBC_Starting_Island
GBC_Broken_Bridge <--- not randomized
GBC_Docks
GBC_Mycologist_Hut
GBC_Temple_Undead
GBC_Temple_Nature
GBC_Temple_Tech
GBC_Temple_Wizard

# actual room names

GBC_WorldMap
GBC_Starting_Island/Room
GBC_Broken_Bridge/Room <--- not randomized
GBC_Docks/Room
GBC_Mycologist_Hut/Room

GBC_Temple_Nature/Temple/OutdoorsCentral
GBC_Temple_Nature/Temple/Pond
GBC_Temple_Nature/Temple/PondSecret
GBC_Temple_Nature/Temple/Meadow
GBC_Temple_Nature/Temple/Woodcarver
GBC_Temple_Nature/Temple/Cabin
GBC_Temple_Nature/Temple/Shop

GBC_Temple_Undead/Temple/MainRoom
GBC_Temple_Undead/Temple/ShopRoom
GBC_Temple_Undead/Temple/MirrorRoom
GBC_Temple_Undead/Temple/BasementRoom
GBC_Temple_Undead/Temple/BonelordRoom_Stairs
GBC_Temple_Undead/Temple/BonelordRoom_1
GBC_Temple_Undead/Temple/BonelordRoom_2

GBC_Temple_Tech/Temple/--- MainRoom ---
GBC_Temple_Tech/Temple/--- Shop ---
GBC_Temple_Tech/Temple/--- AssemblyRoom ---
GBC_Temple_Tech/Temple/--- SmelterRoom ---
GBC_Temple_Tech/Temple/--- DredgingRoom ---

GBC_Temple_Wizard/Temple/Floor_1
GBC_Temple_Wizard/Temple/ShopRoom
GBC_Temple_Wizard/Temple/BackRoom_1
GBC_Temple_Wizard/Temple/Plane_1
GBC_Temple_Wizard/Temple/Floor_2
GBC_Temple_Wizard/Temple/Plane_2
GBC_Temple_Wizard/Temple/Floor_3
GBC_Temple_Wizard/Temple/BackRoom_3
GBC_Temple_Wizard/Temple/Plane_3
GBC_Temple_Wizard/Temple/Floor_4
GBC_Temple_Wizard/Temple/BathRoom

# entrance names

Starting Island Exit: GBC_Starting_Island/Room/ReturnToMapVolume
World Map Starting Island: GBC_WorldMap/????
World Map Broken Bridge: GBC_WorldMap/???? <--- not randomized
Broken Bridge Exit: GBC_Broken_Bridge/Room/ReturnToMapVolume <--- not randomized
World Map Docks: GBC_WorldMap/????
Docks Exit: GBC_Docks/Room/ReturnToMapVolume
World Map Mycologists' Hut: GBC_WorldMap/????
Mycologists' Hut Exit: GBC_Mycologist_Hut/Room/ReturnToMapVolume

World Map Forest: GBC_WorldMap/????
Forest Exit: GBC_Temple_Nature/Temple/OutdoorsCentral/ReturnToMapVolume
Forest Past Prospector: GBC_Temple_Nature/Temple/OutdoorsCentral/Floor/ChangeRoomVolume_Pond
Pond Exit: GBC_Temple_Nature/Temple/Pond/Floor/ChangeRoomVolume
Pond Hidden Path: GBC_Temple_Nature/Temple/Pond/ChangeRoomVolume
Pond OLD_DATA Room Exit: GBC_Temple_Nature/Temple/PondSecret/ChangeRoomVolume
Forest Past Angler: GBC_Temple_Nature/Temple/OutdoorsCentral/Floor/ChangeRoomVolume_Meadow
Meadow Exit: GBC_Temple_Nature/Temple/Meadow/Floor/ChangeRoomVolume
Meadow Hidden Path: GBC_Temple_Nature/Temple/Meadow/ChangeRoomVolume
Woodcarver Room Exit: GBC_Temple_Nature/Temple/Woodcarver/ChangeRoomVolume
Forest Cabin Entrance: GBC_Temple_Nature/Temple/OutdoorsCentral/Cabin/ChangeRoomVolume
Cabin Exit: GBC_Temple_Nature/Temple/Cabin/Floor/ChangeRoomVolume_Outdoors
Cabin Shop Entrance: GBC_Temple_Nature/Temple/Cabin/Floor/ChangeRoomVolume_Outdoors
Cabin Shop Exit: GBC_Temple_Nature/Temple/Shop/Floor/ChangeRoomVolume

World Map Crypt: GBC_WorldMap/????
Crypt Exit: GBC_Temple_Undead/Temple/MainRoom/Floor/ReturnToMapVolume
Crypt Shop Entrance: GBC_Temple_Undead/Temple/MainRoom/Floor/ChangeRoomVolume
Crypt Shop Exit: GBC_Temple_Undead/Temple/ShopRoom/Floor/ChangeRoomVolume
Crypt Mirror Room Entrance: GBC_Temple_Undead/Temple/MainRoom/Floor/ChangeRoomVolume <--- the exact same as shop???
Crypt Mirror Room Exit: GBC_Temple_Undead/Temple/MirrorRoom/Floor/ChangeRoomVolume
Crypt Basement Entrance: GBC_Temple_Undead/Temple/MainRoom/StaircaseVolume
Crypt Basement Exit: GBC_Temple_Undead/Temple/BasementRoom/StaircaseUpVolume
Crypt Basement Bone Lord Stairs: GBC_Temple_Undead/Temple/BasementRoom/StaircaseVolume
Bone Lord Stairs Down: GBC_Temple_Undead/Temple/BonelordRoom_Stairs/StaircaseDownVolume
Bone Lord Stairs Up: GBC_Temple_Undead/Temple/BonelordRoom_Stairs/StaircaseUpVolume
Lower Bone Lord Room Exit Stairs: GBC_Temple_Undead/Temple/BonelordRoom_1/StaircaseUpVolume
Lower Bone Lord Room to Upper: GBC_Temple_Undead/Temple/BonelordRoom_1/ChangeRoomVolume
Upper Bone Lord Room to Lower: GBC_Temple_Undead/Temple/BonelordRoom_2/ChangeRoomVolume

World Map Factory: GBC_WorldMap/????
Factory Exit: GBC_Temple_Tech/Temple/--- MainRoom ---/Floor/ReturnToMapVolume
Factory Shop Entrance: GBC_Temple_Tech/Temple/--- MainRoom ---/Floor/ChangeRoomVolume_Shop
Factory Shop Exit: GBC_Temple_Tech/Temple/--- Shop ---/Floor/ChangeRoomVolume
Factory to Inspection Room: GBC_Temple_Tech/Temple/--- MainRoom ---/Floor/ChangeRoomVolume
Inspection Room Exit: GBC_Temple_Tech/Temple/--- AssemblyRoom ---/Floor/ChangeRoomVolume
Inspection Room to Smelting Room: GBC_Temple_Tech/Temple/--- AssemblyRoom ---/Floor/ChangeRoomVolume
Smelting Room Exit: GBC_Temple_Tech/Temple/--- SmelterRoom ---/Floor/ChangeRoomVolume
Smelting Room Elevator Down: GBC_Temple_Tech/Temple/--- SmelterRoom ---/Objects/Elevator/HoleVolume/DropEndPoint (Elevator/ExitPoint for the point where you end up after entering from elevator)
Dredging Room Elevator Up: GBC_Temple_Tech/Temple/--- DredgingRoom ---/Objects/Elevator/HoleVolume/DropStartPoint

World Map Tower: GBC_WorldMap/????
Tower 1st Floor Exit: GBC_Temple_Wizard/Temple/Floor_1/Floor/ReturnToMapVolume
Tower 1st Floor Shop Entrance: GBC_Temple_Wizard/Temple/Floor_1/Floor/ChangeRoomVolume_Shop/ChangeRoomVolume_Shop
Tower Shop Exit: GBC_Temple_Wizard/Temple/ShopRoom/Floor/ChangeRoomVolume
Tower 1st Floor Back Room Left Entrance: GBC_Temple_Wizard/Temple/Floor_1/ChangeRoomVolume
Tower OLD_DATA Room Left Exit: Temple/BackRoom_1/ChangeRoomVolume
Tower 1st Floor Back Room Right Entrance: GBC_Temple_Wizard/Temple/Floor_1/ChangeRoomVolume
Tower OLD_DATA Room Right Exit: Temple/BackRoom_1/ChangeRoomVolume
Tower 1st Floor Green Gem: GBC_Temple_Wizard/Temple/Floor_1/Objects/GemPedestal/????
Goobert's Plane Exit: GBC_Temple_Wizard/Temple/Plane_1/ReturnToRoom
Tower 1st Floor Stairs Up: GBC_Temple_Wizard/Temple/Floor_1/StaircaseUpVolume
Tower 2nd Floor Stairs Down: GBC_Temple_Wizard/Temple/Floor_2/StaircaseDownVolume
Tower 2nd Floor Drop Down: GBC_Temple_Wizard/Temple/Floor_2/HoleVolume (goes to Floor_1/HoleDropDestination)
Tower 2nd Floor Orange Gem: GBC_Temple_Wizard/Temple/Floor_2/Objects/GemPedestal/????
Pike Mage's Plane Exit: GBC_Temple_Wizard/Temple/Plane_2/ReturnToRoom
Tower 2nd Floor Stairs Up: GBC_Temple_Wizard/Temple/Floor_2/StaircaseUpVolume
Tower 3rd Floor Stairs Down: GBC_Temple_Wizard/Temple/Floor_3/StaircaseDownVolume
Tower 3rd Floor Drop Down: GBC_Temple_Wizard/Temple/Floor_3/HoleVolume (goes to Floor_1/HoleDropDestination)
Tower 3rd Floor Back Room Left Entrance: GBC_Temple_Wizard/Temple/Floor_3/ChangeRoomVolume
Tower Dark Room Left Exit: Temple/BackRoom_3/ChangeRoomVolume
Tower 3rd Floor Back Room Right Entrance: GBC_Temple_Wizard/Temple/Floor_3/ChangeRoomVolume
Tower Dark Room Right Exit: Temple/BackRoom_3/ChangeRoomVolume
Tower 3rd Floor Blue Gem: GBC_Temple_Wizard/Temple/Floor_3/Objects/GemPedestal/????
Lonely Wizard's Plane Exit: GBC_Temple_Wizard/Temple/Plane_3/ReturnToRoom
Tower 3rd Floor Stairs Up: GBC_Temple_Wizard/Temple/Floor_3/StaircaseUpVolume
Tower 4th Floor Stairs Down: GBC_Temple_Wizard/Temple/Floor_4/StaircaseDownVolume
Tower 4th Floor Bathroom Entrance: GBC_Temple_Wizard/Temple/Floor_4/Floor/ChangeRoomVolume_Bathroom
Tower Bathroom Exit: GBC_Temple_Wizard/Temple/BathRoom/Floor/ChangeRoomVolume
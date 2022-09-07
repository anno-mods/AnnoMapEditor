using System;
using System.Text;
using static AnnoMapEditor.MapTemplates.Serializing.HexByteUtils;

namespace AnnoMapEditor.MapTemplates.Serializing.A7t.FileCreation
{

    [Obsolete("This class is part of a fully custom a7t XML Exporter. It will be replaced with an Implementation based on FileDBSerializer as soon as possible.")]
    internal class XMLCreatorA7T : XMLCreatorBase
    {
        public XMLCreatorA7T(int mapSize, int usableArea, byte[] nestedData)
        {
            MapSize = mapSize;
            UsableArea = usableArea;
            NestedData = nestedData;
        }

        private int MapSize { get; }
        private int UsableArea { get; }
        private byte[] NestedData { get; }
        public override XMLItem MakeXML()
        {
            XMLSection root = new XMLSection("Content", 0, false);
            root.AddValueChild("FileVersion", Int32ToLittleEndianHex(8));

            var gameSessionManager = root.AddChildSection("GameSessionManager");
            var sessionSettings = gameSessionManager.AddChildSection("SessionSettings");

            sessionSettings.AddValueChild("GlobalAmbientName", StringToUTF16LittleEndianHex("Moderate_01"));
            sessionSettings.AddValueChild("PlayableArea",
                Int32ToLittleEndianHex((MapSize - UsableArea) / 2) +
                Int32ToLittleEndianHex((MapSize - UsableArea) / 2) +
                Int32ToLittleEndianHex(MapSize - ((MapSize - UsableArea) / 2)) +
                Int32ToLittleEndianHex(MapSize - ((MapSize - UsableArea) / 2)));

            gameSessionManager.AddValueChild("SessionRandomManager", null);
            var terrainManager = gameSessionManager.AddChildSection("TerrainManager");

            terrainManager.AddValueChild("WorldSize", Int32ToLittleEndianHex(MapSize) + Int32ToLittleEndianHex(MapSize));
            var heightMap = terrainManager.AddChildSection("HeightMap");

            heightMap.AddValueChild("Width", Int32ToLittleEndianHex(MapSize * 2 + 1));
            heightMap.AddValueChild("Height", Int32ToLittleEndianHex(MapSize * 2 + 1));
            //Height map is 4 * (MapSize * 2 + 1) * (MapSize * 2 + 1), split to (MapSize * 2 + 1) parts for memory saving, but full string is a little faster, but maybe scales badly
            heightMap.AddActionChild("HeightMap",
                new XMLEntryContent(
                    () => new string('0', 4 * (MapSize * 2 + 1)),
                    MapSize * 2 + 1
                    ));

            var sessionCameraManager = gameSessionManager.AddChildSection("SessionCameraManager");
            var editorSavedCameraStates = sessionCameraManager.AddChildSection("EditorSavedCameraStates");

            for (int i = 0; i < 10; i++)
            {
                var noneSection = editorSavedCameraStates.AddChildSection("None");
                var viewSection = noneSection.AddChildSection("View");
                viewSection.AddValueChild("From", FloatToLittleEndianHex(100f) + FloatToLittleEndianHex(100f) + FloatToLittleEndianHex(0f));
                viewSection.AddValueChild("Up", FloatToLittleEndianHex(0f) + FloatToLittleEndianHex(1f) + FloatToLittleEndianHex(0f));
                viewSection.AddValueChild("Direction", FloatToLittleEndianHex(-0.70710677f) + FloatToLittleEndianHex(-0.70710677f) + FloatToLittleEndianHex(0f));

                var projectionSection = noneSection.AddChildSection("Projection");
                projectionSection.AddValueChild("Flags", Int32ToLittleEndianHex(1));
                projectionSection.AddValueChild("NearClip", FloatToLittleEndianHex(0.5f));
            }

            gameSessionManager.AddValueChild("MessageManager", null);
            var gameObjectManager = gameSessionManager.AddChildSection("GameObjectManager");
            gameObjectManager.AddValueChild("GameObjectLabelMap", null);

            gameSessionManager.AddValueChild("SessionParticipantManager", null);
            gameSessionManager.AddValueChild("IslandManager", null);
            gameSessionManager.AddValueChild("SessionCoastManager", null);

            var worldManager = gameSessionManager.AddChildSection("WorldManager");
            var streetMap = worldManager.AddChildSection("StreetMap");
            var streetID = streetMap.AddChildSection("StreetID");
            streetID.AddValueChild("x", Int32ToLittleEndianHex(MapSize));
            streetID.AddValueChild("y", Int32ToLittleEndianHex(MapSize));
            //street val = 2 * MapSize^2
            streetID.AddActionChild("val",
                new XMLEntryContent(
                    () => new string('0', 2 * MapSize * MapSize)
                    ));

            var varMapData = streetMap.AddChildSection("VarMapData");
            varMapData.AddValueChild("SparseEnabled", BoolToHex(true));
            varMapData.AddValueChild("x", Int32ToLittleEndianHex(MapSize));
            varMapData.AddValueChild("y", Int32ToLittleEndianHex(MapSize));

            var blockSection0 = varMapData.AddChildSection("block");
            blockSection0.AddValueChild("mode", BoolToHex(true));
            blockSection0.AddValueChild("x", Int16ToLittleEndianHex(16));
            blockSection0.AddValueChild("y", Int16ToLittleEndianHex(16));
            var bs0default = blockSection0.AddChildSection("default");
            bs0default.AddValueChild("None", null);

            var blockSection1 = varMapData.AddChildSection("block");
            blockSection1.AddValueChild("mode", BoolToHex(false));

            var water = worldManager.AddChildSection("Water");
            water.AddValueChild("x", Int32ToLittleEndianHex(MapSize));
            water.AddValueChild("y", Int32ToLittleEndianHex(MapSize));
            //Water bits = (MapSize/2)^2
            water.AddActionChild("bits",
                new XMLEntryContent(
                    () => new string('0', (MapSize / 2) * (MapSize / 2))
                    ));

            var riverGrid = worldManager.AddChildSection("RiverGrid");
            riverGrid.AddValueChild("x", Int32ToLittleEndianHex(MapSize));
            riverGrid.AddValueChild("y", Int32ToLittleEndianHex(MapSize));
            //RiverGrid bits = (MapSize/2)^2
            riverGrid.AddActionChild("bits",
                new XMLEntryContent(
                    () => new string('0', (MapSize / 2) * (MapSize / 2))
                    ));

            var environmentGridOuter = worldManager.AddChildSection("EnvironmentGrid");
            var environmentGridInner = environmentGridOuter.AddChildSection("EnvironmentGRid"); //sic
            environmentGridInner.AddValueChild("x", Int32ToLittleEndianHex(MapSize / 4));
            environmentGridInner.AddValueChild("y", Int32ToLittleEndianHex(MapSize / 4));
            //EnvironmentGRid val = 2* (MapSize/4)^2
            environmentGridInner.AddActionChild("val",
                new XMLEntryContent(
                    () => new string('0', 2 * (MapSize / 4) * (MapSize / 4))
                    ));

            gameSessionManager.AddValueChild("PathManager", null);
            gameSessionManager.AddValueChild("SessionEconomyManager", null);
            gameSessionManager.AddValueChild("DiscoveryManager", null);
            gameSessionManager.AddValueChild("RegrowManager", null);

            var sessionSoundManager = gameSessionManager.AddChildSection("SessionSoundManager");
            sessionSoundManager.AddValueChild("AmbientMoodSoundHandler", null);
            sessionSoundManager.AddValueChild("EnvironmentHandler", null);

            gameSessionManager.AddValueChild("ActiveTradeManager", null);
            gameSessionManager.AddValueChild("StreetOverlayManager", null);

            var selectionManager = gameSessionManager.AddChildSection("SelectionManager");
            var selectionGroupController = selectionManager.AddChildSection("SelectionGroupController");
            selectionGroupController.AddValueChild("StoredSelections", null);

            gameSessionManager.AddValueChild("IncidentManager", null);
            gameSessionManager.AddValueChild("CameraSequenceManager", null);
            gameSessionManager.AddValueChild("AIUnitManager", null);

            var aiConstructionManager = gameSessionManager.AddChildSection("AIConstructionManager");
            aiConstructionManager.AddValueChild("PlannedSettlements", null);

            gameSessionManager.AddValueChild("AnimalManager", null);
            gameSessionManager.AddValueChild("CSlotManager", null);
            gameSessionManager.AddValueChild("VisitorManager", null);

            var itemSessionManager = gameSessionManager.AddChildSection("ItemSessionManager");
            itemSessionManager.AddValueChild("ItemMap", null);
            itemSessionManager.AddValueChild("ProductItem", null);
            itemSessionManager.AddValueChild("SetBuffs", null);
            itemSessionManager.AddValueChild("SessionBuffs", null);
            itemSessionManager.AddValueChild("AreaBuffs", null);

            gameSessionManager.AddValueChild("MilitaryManager", null);
            gameSessionManager.AddValueChild("BlueprintManager", null);
            gameSessionManager.AddValueChild("LoadingPierManager", null);
            gameSessionManager.AddValueChild("SessionFreeAreaProductivityManager", null);

            var workforceTransferManager = gameSessionManager.AddChildSection("WorkforceTransferManager");
            workforceTransferManager.AddValueChild("ParticipantData", null);

            gameSessionManager.AddValueChild("AreaManager", null);
            gameSessionManager.AddValueChild("AreaLinks", null);

            var areaIDs = gameSessionManager.AddChildSection("AreaIDs");
            areaIDs.AddValueChild("x", Int32ToLittleEndianHex(MapSize));
            areaIDs.AddValueChild("y", Int32ToLittleEndianHex(MapSize));
            //areaIDs val = MapSize^2 x 0100
            areaIDs.AddActionChild("val",
                new XMLEntryContent(
                    () =>
                    {
                        StringBuilder builder = new StringBuilder();
                        string byte1 = ByteToHex((byte)1);
                        string byte0 = ByteToHex((byte)0);
                        int mapSizeDiv64 = MapSize / 64;

                        for (int i = 0; i < 64; i++)
                        {
                            builder.Append(byte1 + byte0);
                        }

                        string pack = builder.ToString();
                        builder = new StringBuilder();

                        for (int i = 0; i < mapSizeDiv64; i++)
                        {
                            builder.Append(pack);
                        }

                        return builder.ToString();
                    })
                    );


            var spawnAreaPoints = gameSessionManager.AddChildSection("SpawnAreaPoints");

            byte[] noneBytes = new byte[] { 12, 10, 9, 14, 8, 7, 15, 6, 13, 4, 3, 11, 2, 1, 5, 0 };
            foreach (byte b in noneBytes)
            {
                spawnAreaPoints.AddValueChild("None", ByteToHex(b));
                var noneSection = spawnAreaPoints.AddChildSection("None");
                noneSection.AddValueChild("m_XSize", Int32ToLittleEndianHex(MapSize));
                noneSection.AddValueChild("m_YSize", Int32ToLittleEndianHex(MapSize));
                noneSection.AddValueChild("m_AreaPointGrid", null);
                noneSection.AddValueChild("m_AreaRect", null);
            }

            var areaManagerData = gameSessionManager.AddChildSection("AreaManagerData");
            areaManagerData.AddValueChild("None", Int16ToLittleEndianHex(1));
            var areaManagerDataNoneSection = areaManagerData.AddChildSection("None");

            //Vanilla nested data is 351 bytes long
            areaManagerDataNoneSection.AddValueChild("ByteCount", Int32ToLittleEndianHex(NestedData.Length));
            areaManagerDataNoneSection.AddValueChild("Data", BytesToContinuousHex(NestedData));


            return root;
        }
    }
}

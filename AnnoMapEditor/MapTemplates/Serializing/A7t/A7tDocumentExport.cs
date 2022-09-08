using FileDBSerializing.EncodingAwareStrings;
using FileDBSerializing.ObjectSerializer;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Serializing.A7t
{
    public class A7tDocumentExport
    {
        public A7tDocumentExport(int mapSize, int playableSize, string ambientName, byte[] areaManagerData)
        {
            FileVersion = 8;
            GameSessionManager = new GameSessionManager(mapSize, playableSize, ambientName, areaManagerData);
        }

        public int FileVersion { get; set; }
        public GameSessionManager GameSessionManager { get; set; }
    }

    public class GameSessionManager
    {
        public GameSessionManager(int mapSize, int playableSize, string ambientName, byte[] areaManagerData)
        {
            SessionSettings = new SessionSettings(ambientName, mapSize, playableSize);
            SessionrandomManager = new Empty();
            TerrainManager = new TerrainManager(mapSize);
            SessionCameraManager = new SessionCameraManager();
            MessageManager = new Empty();
            GameObjectManager = new GameObjectManager();
            SessionParticipantManager = new Empty();
            IslandManager = new Empty();
            SessionCoastManager = new Empty();
            WorldManager = new WorldManager(mapSize);
            PathManager = new Empty();
            SessionEconomyManager = new Empty();
            DiscoveryManager = new Empty();
            RegrowManager = new Empty();
            SessionSoundManager = new SessionSoundManager();
            ActiveTradeManager = new Empty();
            StreetOverlayManager = new Empty();
            SelectionManager = new SelectionManager();
            IncidentManager = new Empty();
            CameraSequenceManager = new Empty();
            AIUnitManager = new Empty();
            AIConstructionManager = new AIConstructionManager();
            AnimalManager = new Empty();
            CSlotManager = new Empty();
            VisitorManager = new Empty();
            ItemSessionManager = new ItemSessionManager();
            MilitaryManager = new Empty();
            BlueprintManager = new Empty();
            LoadingPierManager = new Empty();
            SessionFreeAreaProductivityManager = new Empty();
            WorkforceTransferManager = new WorkforceTransferManager();
            AreaManager = new Empty();
            AreaLinks = new Empty();
            AreaIDs = new AreaIDs(mapSize);
            SpawnAreaPoints = new SpawnAreaPoints(mapSize);
            AreaManagerData = new AreaManagerData(areaManagerData);
        }

        public SessionSettings SessionSettings { get; set; }
        public Empty SessionrandomManager { get; set; }
        public TerrainManager TerrainManager { get; set; }
        public SessionCameraManager SessionCameraManager { get; set; }
        public Empty MessageManager { get; set; }
        public GameObjectManager GameObjectManager { get; set; }
        public Empty SessionParticipantManager { get; set; }
        public Empty IslandManager { get; set; }
        public Empty SessionCoastManager { get; set; }
        public WorldManager WorldManager { get; set; }
        public Empty PathManager { get; set; }
        public Empty SessionEconomyManager { get; set; }
        public Empty DiscoveryManager { get; set; }
        public Empty RegrowManager { get; set; }
        public SessionSoundManager SessionSoundManager { get; set; }
        public Empty ActiveTradeManager { get; set; }
        public Empty StreetOverlayManager { get; set; }
        public SelectionManager SelectionManager { get; set; }
        public Empty IncidentManager { get; set; }
        public Empty CameraSequenceManager { get; set; }
        public Empty AIUnitManager { get; set; }
        public AIConstructionManager AIConstructionManager { get; set; }
        public Empty AnimalManager { get; set; }
        public Empty CSlotManager { get; set; }
        public Empty VisitorManager { get; set; }
        public ItemSessionManager ItemSessionManager { get; set; }
        public Empty MilitaryManager { get; set; }
        public Empty BlueprintManager { get; set; }
        public Empty LoadingPierManager { get; set; }
        public Empty SessionFreeAreaProductivityManager { get; set; }
        public WorkforceTransferManager WorkforceTransferManager { get; set; }
        public Empty AreaManager { get; set; }
        public Empty AreaLinks { get; set; }
        public AreaIDs AreaIDs { get; set; }
        public SpawnAreaPoints SpawnAreaPoints { get; set; }
        public AreaManagerData AreaManagerData { get; set; }


    }

    public class SessionSettings
    {
        public SessionSettings(string ambientName, int mapSize, int playableSize)
        {
            int margin = (mapSize - playableSize) / 2;
            PlayableArea = new int[] { margin, margin, playableSize + margin, playableSize + margin };

            GlobalAmbientName = ambientName;
        }

        public UnicodeString GlobalAmbientName { get; set; }

        public int[] PlayableArea { get; set; }
    }

    public class TerrainManager
    {
        public TerrainManager(int mapSize)
        {
            WorldSize = new int[] { mapSize, mapSize };
            HeightMap = new HeightMapElement(mapSize);
        }

        public int[] WorldSize { get; set; }

        public HeightMapElement HeightMap { get; set; }
    }

    public class HeightMapElement
    {
        public HeightMapElement(int mapSize)
        {
            int heightMapSize = (2 * mapSize) + 1;
            Width = heightMapSize;
            Height = heightMapSize;
            HeightMap = Enumerable.Repeat((short)0, heightMapSize * heightMapSize).ToArray();
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public short[] HeightMap { get; set; }
    }


    #region SessionCameraManager
    public class SessionCameraManager
    {
        public SessionCameraManager()
        {
            EditorSavedCameraStates = new EditorSavedCameraStates();
        }

        public EditorSavedCameraStates EditorSavedCameraStates { get; set; }
    }

    public class EditorSavedCameraStates
    {
        public EditorSavedCameraStates()
        {
            None = Enumerable.Repeat(false, 10).Select(dummy => new None_EditorSavedCameraState()).ToArray();
        }

        [FlatArray]
        public None_EditorSavedCameraState[] None { get; set; }
    }

    public class None_EditorSavedCameraState
    {
        public None_EditorSavedCameraState()
        {
            View = new View();
            Projection = new Projection();
        }

        public View View { get; set; }
        public Projection Projection { get; set; }
    }

    public class View
    {
        public View()
        {
            From = new float[] { 100f, 100f, 0f };
            Up = new float[] { 0f, 1f, 0f };
            Direction = new float[] { -0.70710677f, -0.70710677f, 0f };
        }

        public float[] From { get; set; }
        public float[] Up { get; set; }
        public float[] Direction { get; set;}
    }

    public class Projection
    {
        public Projection()
        {
            Flags = 1;
            NearClip = 0.5f;
        }

        public int Flags { get; set; }
        public float NearClip { get; set; }
    }
    #endregion


    public class GameObjectManager
    {
        public GameObjectManager()
        {
            GameObjectLabelMap = new Empty();
        }

        public Empty GameObjectLabelMap { get; set; }
    }

    #region WorldManager

    public class WorldManager
    {
        public WorldManager(int mapSize)
        {
            StreetMap = new StreetMap(mapSize);
            Water = new Water(mapSize);
            RiverGrid = new RiverGrid(mapSize);
            EnvironmentGrid = new EnvironmentGrid(mapSize);
        }

        public StreetMap StreetMap { get; set; }
        public Water Water { get; set; }
        public RiverGrid RiverGrid { get; set; }
        public EnvironmentGrid EnvironmentGrid { get; set; }
    }

    public class StreetMap
    {
        public StreetMap(int mapSize)
        {
            StreetID = new StreetID(mapSize);
            VarMapData = new VarMapData(mapSize);
        }

        public StreetID StreetID { get; set; }
        public VarMapData VarMapData { get; set; }
    }

    public class StreetID
    {
        public StreetID(int mapSize)
        {
            x = mapSize;
            y = mapSize;
            val = Enumerable.Repeat((byte)0, mapSize * mapSize).ToArray(); ;
        }

        public int x { get; set; }
        public int y { get; set; }

        public byte[] val { get; set; }
    }

    public class VarMapData
    {
        public VarMapData(int mapSize)
        {
            SparseEnabled = 1;
            x = mapSize;
            y = mapSize;
            block = new blockBase[] { new blockExtended(1), new blockEmpty(0) };
        }

        public byte SparseEnabled { get; set; }
        public int x { get; set; }
        public int y { get; set; }

        [FlatArray]
        public blockBase[] block { get; set; }
    }

    public abstract class blockBase
    {
        public blockBase(byte mode)
        {
            this.mode = mode;
        }

        public byte mode { get; set; }
    }

    public class blockEmpty : blockBase
    {
        public blockEmpty(byte mode) : base(mode)
        {

        }
    }

    public class blockExtended : blockBase
    {
        public blockExtended(byte mode) : base(mode)
        {
            x = 16;
            y = 16;
            defaultElem = new defaultElement();
        }

        public short x { get; set; }
        public short y { get; set; }

        //TODO: Name Change annotation
        public defaultElement defaultElem { get; set; }
    }

    public class defaultElement
    {
        public defaultElement()
        {
            None = new Empty();
        }

        public Empty None { get; set; }
    }

    public class Water
    {
        public Water(int mapSize)
        {
            x = mapSize;
            y = mapSize;
            bits = Enumerable.Repeat((byte)0, mapSize * mapSize / 4).ToArray();
        }

        public int x { get; set; }
        public int y { get; set; }

        public byte[] bits { get; set; }
    }

    public class RiverGrid
    {
        public RiverGrid(int mapSize)
        {
            x = mapSize;
            y = mapSize;
            bits = Enumerable.Repeat((byte)0, mapSize * mapSize / 4).ToArray();
        }

        public int x { get; set; }
        public int y { get; set; }

        public byte[] bits { get; set; }
    }

    public class EnvironmentGrid
    {
        public EnvironmentGrid(int mapSize)
        {
            EnvironmentGRid = new EnvironmentGRid(mapSize);
        }

        //sic
        public EnvironmentGRid EnvironmentGRid { get; set; }
    }

    //sic
    public class EnvironmentGRid
    {
        public EnvironmentGRid(int mapSize)
        {
            int quarterMapSize = mapSize / 4;

            x = quarterMapSize;
            y = quarterMapSize;

            val = Enumerable.Repeat((byte)0, quarterMapSize * quarterMapSize).ToArray();
        }

        public int x { get; set; }
        public int y { get; set; }

        public byte[] val { get; set; }
    }

    #endregion

    public class SessionSoundManager
    {
        public SessionSoundManager()
        {
            AmbientMoodSoundHandler = new Empty();
            EnvironmentHandler = new Empty();
        }

        public Empty AmbientMoodSoundHandler { get; set; }
        public Empty EnvironmentHandler { get; set; }
    }

    public class SelectionManager
    {
        public SelectionManager()
        {
            SelectionGroupController = new SelectionGroupController();
        }

        public SelectionGroupController SelectionGroupController { get; set; }
    }

    public class SelectionGroupController
    {
        public SelectionGroupController()
        {
            StoredSelections = new Empty();
        }

        public Empty StoredSelections { get; set; }
    }

    public class AIConstructionManager
    {
        public AIConstructionManager()
        {
            PlannedSettlements = new Empty();
        }

        public Empty PlannedSettlements { get; set; }
    }

    public class ItemSessionManager
    {
        public ItemSessionManager()
        {
            ItemMap = new Empty();
            ProductItem = new Empty();
            SetBuffs = new Empty();
            SessionBuffs = new Empty();
            AreaBuffs = new Empty();
        }

        public Empty ItemMap { get; set; }
        public Empty ProductItem { get; set; }
        public Empty SetBuffs { get; set; }
        public Empty SessionBuffs { get; set; }
        public Empty AreaBuffs { get; set; }
    }

    public class WorkforceTransferManager
    {
        public WorkforceTransferManager()
        {
            ParticipantData = new Empty();
        }

        public Empty ParticipantData { get; set; }
    }

    public class AreaIDs
    {
        public AreaIDs(int mapSize)
        {
            x = mapSize;
            y = mapSize;
            val = Enumerable.Repeat((short)1, mapSize * mapSize).ToArray(); ;
        }

        public int x { get; set; }
        public int y { get; set; }

        public short[] val { get; set; }
    }

    public class SpawnAreaPoints
    {
        public SpawnAreaPoints(int mapSize)
        {
            byte[] noneBytes = new byte[] { 12, 10, 9, 14, 8, 7, 15, 6, 13, 4, 3, 11, 2, 1, 5, 0 };

            //TODO: Tuple Flattening Feature required
            None = new object[noneBytes.Length * 2];
            for (int i = 0; i < noneBytes.Length; i++)
            {
                None[i * 2] = noneBytes[i];
                None[i * 2 + 1] = new None_Area_SpawnAreaPoints(mapSize);
            }
        }

        [FlatArray]
        public object[] None { get; set; }
    }

    public class None_Area_SpawnAreaPoints
    {
        public None_Area_SpawnAreaPoints(int mapSize)
        {
            m_XSize = mapSize;
            m_YSize = mapSize;
            m_AreaPointGrid = new Empty();
            m_AreaRect = new Empty();
        }

        public int m_XSize { get; set; }
        public int m_YSize { get; set; }
        public Empty m_AreaPointGrid { get; set; }
        public Empty m_AreaRect { get; set; }
    }

    public class AreaManagerData
    {
        public AreaManagerData(byte[] data)
        {
            //TODO: Tuple Flattening Feature required
            None = new object[2];
            None[0] = (short)1;
            None[1] = new None_AreaManagerSubdata(data);
        }

        [FlatArray]
        public object[] None { get; set; }
    }

    public class None_AreaManagerSubdata
    {
        public None_AreaManagerSubdata(byte[] data)
        {
            ByteCount = data.Length;
            Data = data;
        }

        //Check Bytecount: Vanilla == 351
        public int ByteCount { get; set; }
        public byte[] Data { get; set; }

    }

}

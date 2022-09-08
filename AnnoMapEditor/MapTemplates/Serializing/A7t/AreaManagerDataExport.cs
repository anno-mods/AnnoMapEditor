using System;

namespace AnnoMapEditor.MapTemplates.Serializing.A7t
{
    public class AreaManagerDataExport
    {
        public AreaManagerDataExport()
        {
            AreaObjectManager = new AreaObjectManager();
        }

        public AreaObjectManager AreaObjectManager { get; set; }
    }

    public class AreaObjectManager
    {
        public AreaObjectManager()
        {
            GameObjectIDCounter = 0;
            NonGameObjectIDCounter = 0;
            QueuedChangeGUID = new Empty();
            QueuedDeletes = "";
            ObjectGroupFilterCollection = new ObjectGroupFilterCollection();
            ObjectGroupCollection = new ObjectGroupCollection();
            GameObject = new GameObject();
            NaturePreset = new NaturePreset();
            EditorObject = new EditorObject();
        }

        public long GameObjectIDCounter { get; set; }
        public long NonGameObjectIDCounter { get; set; }
        public Empty QueuedChangeGUID { get; set; }
        public string QueuedDeletes { get; set; }
        public ObjectGroupFilterCollection ObjectGroupFilterCollection { get; set; }
        public ObjectGroupCollection ObjectGroupCollection { get; set; }
        public GameObject GameObject { get; set; }
        public NaturePreset NaturePreset { get; set; }
        public EditorObject EditorObject { get; set; }
    }

    public class ObjectGroupFilterCollection
    {
        public ObjectGroupFilterCollection()
        {
            Root = new Root();
        }

        public Root Root { get; set; }
    }

    public class Root
    {
        public Root()
        {
            Filter = new Filter();
        }

        public Filter Filter { get; set; }
    }

    public class Filter
    {
        public Filter()
        {
            None = new None_Filter();
        }

        public None_Filter None { get; set; }
    }

    public class None_Filter
    {
        public None_Filter()
        {
            FolderID = 1;
            Filter = new Empty();
        }

        public int FolderID { get; set; }
        public Empty Filter { get; set; }

    }

    public class ObjectGroupCollection
    {
        public ObjectGroupCollection()
        {
            ObjectGroups = new Empty();
        }

        public Empty ObjectGroups { get; set; }
    }

    public class GameObject
    {
        public GameObject()
        {
            objects = new Empty();
        }

        public Empty objects { get; set; }
    }

    public class NaturePreset
    {
        public NaturePreset()
        {
            objects = new Empty();
        }

        public Empty objects { get; set; }
    }

    public class EditorObject
    {
        public EditorObject()
        {
            objects = new Empty();
        }

        public Empty objects { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Serializing;

namespace AnnoMapEditor.MapTemplates
{
    public class Session
    {
        public IReadOnlyList<MapElement> Elements => _elements;
        private List<MapElement> _elements;

        public Vector2 Size { get; private set; }
        public Rect2 PlayableArea { get; private set; }
        public Region Region { get; private set; }

        private MapTemplateDocument template;

        public event NotifyCollectionChangedEventHandler? ElementCollectionChanged;
        public event EventHandler<SessionResizeEventArgs>? MapSizeConfigChanged;
        public event EventHandler? MapSizeConfigCommitted;

        public string MapSizeText => $"Size: {Size.X}, Playable: {PlayableArea.Width}";

        public Session()
        {
            Size = new Vector2(0, 0);
            _elements = new List<MapElement>();
            template = new MapTemplateDocument();
        }

        private static Region DetectRegionFromPath(string filePath)
        {
            if (filePath.Contains("colony01"))
                return Region.NewWorld;
            else if (filePath.Contains("dlc03") || filePath.Contains("colony_03"))
                return Region.Arctic;
            else if (filePath.Contains("dlc06") || filePath.Contains("colony02"))
                return Region.Enbesa;
            return Region.Moderate;
        }

        public static async Task<Session?> FromA7tinfoAsync(string filePath)
        {
            return await FromA7tinfoAsync(File.OpenRead(filePath), filePath);
        }

        public static async Task<Session?> FromA7tinfoAsync(Stream? stream, string internalPath)
        {
            var doc = await Serializer.ReadAsync<MapTemplateDocument>(stream);
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, DetectRegionFromPath(internalPath));
        }

        public static async Task<Session?> FromXmlAsync(string filePath)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(File.OpenRead(filePath));
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, DetectRegionFromPath(filePath));
        }

        public static async Task<Session?> FromXmlAsync(Stream? stream, string internalPath)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(stream);
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, DetectRegionFromPath(internalPath));
        }

        public static Session? FromTemplateDocument(MapTemplateDocument document, Region region)
        {
            var islands = from element in document.MapTemplate?.TemplateElement
                          where element?.Element is not null
                          select Island.FromSerialized(element, region);
            Session session = new()
            {
                Region = region,
                _elements = new List<MapElement>(islands),
                Size = new Vector2(document.MapTemplate?.Size),
                PlayableArea = new Rect2(document.MapTemplate?.PlayableArea),
                template = document
            };

            // clear links in the original
            if (session.template.MapTemplate is not null)
                session.template.MapTemplate.TemplateElement = null;

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public static Session? FromNewMapDimensions(int mapSize, int playableSize, Region region)
        {
            int margin = (mapSize - playableSize) / 2;

            MapTemplateDocument createdTemplateDoc = new MapTemplateDocument()
            {
                MapTemplate = new MapTemplate()
                {
                    Size = new int[] { mapSize, mapSize },
                    PlayableArea = new int[] { margin, margin, playableSize + margin, playableSize + margin },
                    ElementCount = 4
                }
            };

            Session session = new()
            {
                Region = region,
                Size = new Vector2(createdTemplateDoc.MapTemplate.Size),
                PlayableArea = new Rect2(createdTemplateDoc.MapTemplate.PlayableArea),
                template = createdTemplateDoc
            };

            foreach (MapElement startingSpot in StartingSpot.CreateStartingSpots(playableSize, margin))
                session.AddElement(startingSpot);

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public void ResizeSession(int mapSize, (int x1, int y1, int x2, int y2) playableAreaMargins)
        {
            Vector2 oldMapSize = new Vector2(Size);
            Size = new Vector2(mapSize, mapSize);

            Vector2 oldPlayableSize = new Vector2(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new Rect2(new int[] { playableAreaMargins.x1, playableAreaMargins.y1, playableAreaMargins.x2, playableAreaMargins.y2 });

            MapSizeConfigChanged?.Invoke(this, new SessionResizeEventArgs(oldMapSize, oldPlayableSize));
        }

        public void ResizeAndCommitSession(int mapSize, (int x1, int y1, int x2, int y2) playableAreaMargins)
        {
            //Commit means write to template
            template.MapTemplate.Size = new int[] { mapSize, mapSize };
            template.MapTemplate.PlayableArea = new int[] { 
                playableAreaMargins.x1, 
                playableAreaMargins.y1, 
                playableAreaMargins.x2, 
                playableAreaMargins.y2 
            };

            Vector2 oldMapSize = new Vector2(Size);
            Size = new Vector2(template.MapTemplate.Size);

            Vector2 oldPlayableSize = new Vector2(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new Rect2(template.MapTemplate.PlayableArea);

            MapSizeConfigChanged?.Invoke(this, new SessionResizeEventArgs(oldMapSize, oldPlayableSize));
            MapSizeConfigCommitted?.Invoke(this, new EventArgs());
        }

        public void SetRegion(Region region)
        {
            Region = region;
            foreach (var element in Elements)
                if (element is Island island)
                    island.SetRegion(region);
        }

        public MapTemplateDocument? ToTemplate(bool writeInitialArea = false)
        {
            if (template.MapTemplate?.Size is null || template.MapTemplate?.PlayableArea is null)
                return null;

            template.MapTemplate.TemplateElement = new List<TemplateElement>(Elements.Select(x => x.ToTemplate()).Where(x => x is not null)!);
            template.MapTemplate.ElementCount = template.MapTemplate.TemplateElement.Count;

            if (Region.HasMapExtension && writeInitialArea)
                template.MapTemplate.InitialPlayableArea = template.MapTemplate.PlayableArea;
            else
                template.MapTemplate.InitialPlayableArea = null;

            return template;
        }

        public async Task SaveToXmlAsync(string filePath)
        {
            var export = ToTemplate();
            if (export is null)
                return;

            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await Serializer.WriteToXmlAsync(export, file);
        }

        public async Task SaveAsync(string filePath, bool writeInitialArea)
        {
            var export = ToTemplate();
            if (export is null)
                return;

            var parentPath = Path.GetDirectoryName(filePath);
            if (parentPath is not null)
                Directory.CreateDirectory(parentPath);
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await Serializer.WriteAsync(export, file);
        }

        public void AddElement(MapElement mapElement)
        {
            _elements.Add(mapElement);

            if (mapElement is Island island)
            {
                island.CreateTemplate();
                island.Init();
            }

            ElementCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveElement(MapElement element)
        {
            _elements.Remove(element);
            ElementCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public class SessionResizeEventArgs : EventArgs
        {
            public SessionResizeEventArgs(Vector2 oldMapSize, Vector2 oldPlayableSize)
            {
                OldMapSize = new Vector2(oldMapSize);
                OldPlayableSize = new Vector2(oldPlayableSize);
            }

            public Vector2 OldMapSize { get; }
            public Vector2 OldPlayableSize { get; }
        }
    }
}

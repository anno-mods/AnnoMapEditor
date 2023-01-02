using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.MapTemplates
{
    public class Session : ObservableBase
    {
        public ObservableCollection<Island> Islands { get; } = new();

        public Vector2 Size 
        { 
            get => _size; 
            private set => SetProperty(ref _size, value, new[] { nameof(MapSizeText) }); 
        }
        private Vector2 _size = Vector2.Zero;

        public Rect2 PlayableArea 
        {
            get => _playableArea;
            private set => SetProperty(ref _playableArea, value, new[] { nameof(MapSizeText) });
        }
        private Rect2 _playableArea = new();

        public Region Region 
        { 
            get => _region;
            set => SetProperty(ref _region, value);
        }
        private Region _region;

        private MapTemplateDocument _template = new();

        public event EventHandler<SessionResizeEventArgs>? MapSizeConfigChanged;
        public event EventHandler? MapSizeConfigCommitted;

        public string MapSizeText => $"Size: {Size.X}, Playable: {PlayableArea.Width}";


        public Session(Region region)
        {
            _region = region;
            _template = new MapTemplateDocument();

            Islands.CollectionChanged += Islands_CollectionChanged;
        }

        private void Islands_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (object newItem in e.NewItems)
                    if (newItem is Island island && island.IsNew)
                    {
                        if (island.IsNew)
                            island.CreateTemplate();

                        island.Init();
                    }
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

            return FromTemplateDocument(doc, Region.DetectFromPath(internalPath));
        }

        public static async Task<Session?> FromXmlAsync(string filePath)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(File.OpenRead(filePath));
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, Region.DetectFromPath(filePath));
        }

        public static async Task<Session?> FromXmlAsync(Stream? stream, string internalPath)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(stream);
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, Region.DetectFromPath(internalPath));
        }

        public static Session? FromTemplateDocument(MapTemplateDocument document, Region region)
        {
            Session session = new(region)
            {
                Size = new Vector2(document.MapTemplate?.Size),
                PlayableArea = new Rect2(document.MapTemplate?.PlayableArea),
                _template = document
            };

            IEnumerable<Island> islands = from element in document.MapTemplate?.TemplateElement
                              where element?.Element is not null
                              select Island.FromSerialized(element, region);
            session.Islands.AddRange(islands);

            // clear links in the original
            if (session._template.MapTemplate is not null)
                session._template.MapTemplate.TemplateElement = null;

            // number starting positions
            int counter = 0;
            foreach (var island in session.Islands)
                if (island.IsStarter)
                    island.Counter = counter++;

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public static Session? FromNewMapDimensions(int mapSize, int playableSize, Region region)
        {
            int margin = (mapSize - playableSize) / 2;

            List<Island> startingSpots = CreateNewStartingSpots(mapSize, region);

            MapTemplateDocument createdTemplateDoc = new MapTemplateDocument()
            {
                MapTemplate = new MapTemplate()
                {
                    Size = new int[] { mapSize, mapSize },
                    PlayableArea = new int[] { margin, margin, playableSize + margin, playableSize + margin },
                    ElementCount = 4
                }
            };

            Session session = new(region)
            {
                Size = new Vector2(createdTemplateDoc.MapTemplate.Size),
                PlayableArea = new Rect2(createdTemplateDoc.MapTemplate.PlayableArea),
                _template = createdTemplateDoc
            };

            int startingSpotCounter = 0;
            foreach (Island startingSpot in startingSpots)
            {
                startingSpot.Counter = startingSpotCounter++;
                session.Islands.Add(startingSpot);
            }

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public static List<Island> CreateNewStartingSpots(int mapSize, Region region)
        {
            const int SPACING = 32;
            int halfSize = mapSize / 2;

            List<Island> starts = new List<Island>()
            {
                new(region)
                {
                    ElementType = MapElementType.StartingSpot,
                    Position = new(halfSize + SPACING, halfSize + SPACING)
                },
                new(region)
                {
                    ElementType = MapElementType.StartingSpot,
                    Position = new(halfSize + SPACING, halfSize - SPACING) 
                },
                new(region)
                {
                    ElementType = MapElementType.StartingSpot,
                    Position = new(halfSize - SPACING, halfSize - SPACING) 
                },
                new(region)
                {
                    ElementType = MapElementType.StartingSpot,
                    Position = new(halfSize - SPACING, halfSize + SPACING) 
                }
            };

            return starts;
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
            _template.MapTemplate.Size = new int[] { mapSize, mapSize };
            _template.MapTemplate.PlayableArea = new int[] { 
                playableAreaMargins.x1, 
                playableAreaMargins.y1, 
                playableAreaMargins.x2, 
                playableAreaMargins.y2 
            };

            Vector2 oldMapSize = new Vector2(Size);
            Size = new Vector2(_template.MapTemplate.Size);

            Vector2 oldPlayableSize = new Vector2(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new Rect2(_template.MapTemplate.PlayableArea);

            MapSizeConfigChanged?.Invoke(this, new SessionResizeEventArgs(oldMapSize, oldPlayableSize));
            MapSizeConfigCommitted?.Invoke(this, new EventArgs());
        }

        public void UpdateExternalData()
        {
            foreach (var island in Islands)
                island.UpdateExternalData();
        }

        public void Update()
        {
            foreach (var island in Islands)
                island.Init();
        }

        public MapTemplateDocument? ToTemplate(bool writeInitialArea = false)
        {
            if (_template.MapTemplate?.Size is null || _template.MapTemplate?.PlayableArea is null)
                return null;

            _template.MapTemplate.TemplateElement = new List<TemplateElement>(Islands.Select(x => x.ToTemplate()).Where(x => x is not null)!);
            _template.MapTemplate.ElementCount = _template.MapTemplate.TemplateElement.Count;

            if (Region.HasMapExtension && writeInitialArea)
                _template.MapTemplate.InitialPlayableArea = _template.MapTemplate.PlayableArea;
            else
                _template.MapTemplate.InitialPlayableArea = null;

            return _template;
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

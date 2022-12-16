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
        public IReadOnlyList<Island> Islands => _islands;
        private List<Island> _islands;

        public Vector2 Size { get; private set; }
        public Rect2 PlayableArea { get; private set; }
        public Region Region { get; set; }

        private MapTemplateDocument template;

        public event NotifyCollectionChangedEventHandler? IslandCollectionChanged;
        public event EventHandler<SessionResizeEventArgs>? MapSizeConfigChanged;
        public event EventHandler? MapSizeConfigCommitted;

        public string MapSizeText => $"Size: {Size.X}, Playable: {PlayableArea.Width}";

        public Session()
        {
            Size = new Vector2(0, 0);
            _islands = new List<Island>();
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

            return await FromTemplateDocument(doc, DetectRegionFromPath(internalPath));
        }

        public static async Task<Session?> FromXmlAsync(string filePath)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(File.OpenRead(filePath));
            if (doc is null)
                return null;

            return await FromTemplateDocument(doc, DetectRegionFromPath(filePath));
        }

        public static async Task<Session?> FromXmlAsync(Stream? stream, string internalPath)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(stream);
            if (doc is null)
                return null;

            return await FromTemplateDocument(doc, DetectRegionFromPath(internalPath));
        }

        public static async Task<Session?> FromTemplateDocument(MapTemplateDocument document, Region region)
        {
            var islands = from element in document.MapTemplate?.TemplateElement
                          where element?.Element is not null
                          select Island.FromSerialized(element, region);
            Session session = new()
            {
                Region = region,
                _islands = new List<Island>(await Task.WhenAll(islands)),
                Size = new Vector2(document.MapTemplate?.Size),
                PlayableArea = new Rect2(document.MapTemplate?.PlayableArea),
                template = document
            };

            // clear links in the original
            if (session.template.MapTemplate is not null)
                session.template.MapTemplate.TemplateElement = null;

            // number starting positions
            int counter = 0;
            foreach (var island in session.Islands)
                if (island.ElementType == 2)
                    island.Counter = counter++;

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public static Session? FromNewMapDimensions(int mapSize, int playableSize, Region region)
        {
            int margin = (mapSize - playableSize) / 2;

            List<Island> startingSpots = Island.CreateNewStartingSpots(playableSize, margin, region);

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
                _islands = new List<Island>(),
                Size = new Vector2(createdTemplateDoc.MapTemplate.Size),
                PlayableArea = new Rect2(createdTemplateDoc.MapTemplate.PlayableArea),
                template = createdTemplateDoc
            };


            int startingSpotCounter = 0;
            foreach (Island startingSpot in startingSpots)
            {
                startingSpot.Counter = startingSpotCounter++;
                session.AddIsland(startingSpot);
            }

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public void ResizeSession(int mapSize, int playableSize)
        {
            int margin = (mapSize - playableSize) / 2;

            Vector2 oldMapSize = new Vector2(Size);
            Size = new Vector2(mapSize, mapSize);

            Vector2 oldPlayableSize = new Vector2(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new Rect2(new int[] { margin, margin, playableSize + margin, playableSize + margin });

            MapSizeConfigChanged?.Invoke(this, new SessionResizeEventArgs(oldMapSize, oldPlayableSize));
        }

        public void ResizeAndCommitSession(int mapSize, int playableSize)
        {
            int margin = (mapSize - playableSize) / 2;

            //Commit means write to template
            template.MapTemplate.Size = new int[] { mapSize, mapSize };
            template.MapTemplate.PlayableArea = new int[] { margin, margin, playableSize + margin, playableSize + margin };

            Vector2 oldMapSize = new Vector2(Size);
            Size = new Vector2(template.MapTemplate.Size);

            Vector2 oldPlayableSize = new Vector2(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new Rect2(template.MapTemplate.PlayableArea);

            MapSizeConfigChanged?.Invoke(this, new SessionResizeEventArgs(oldMapSize, oldPlayableSize));
            MapSizeConfigCommitted?.Invoke(this, new EventArgs());
        }

        public async Task UpdateExternalDataAsync()
        {
            foreach (var island in Islands)
                await island.UpdateExternalDataAsync();
        }

        public async Task UpdateAsync()
        {
            foreach (var island in Islands)
                await island.InitAsync(Region);
        }

        public MapTemplateDocument? ToTemplate()
        {
            if (template.MapTemplate?.Size is null || template.MapTemplate?.PlayableArea is null)
                return null;

            template.MapTemplate.TemplateElement = new List<TemplateElement>(Islands.Select(x => x.ToTemplate()).Where(x => x is not null)!);
            template.MapTemplate.ElementCount = template.MapTemplate.TemplateElement.Count;

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

        public async Task SaveAsync(string filePath)
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

        public void AddIsland(Island island)
        {
            island.CreateTemplate();
            _islands.Add(island);
            Task.Run(async () => await island.InitAsync(Region));
            IslandCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveIsland(Island island)
        {
            _islands.Remove(island);
            IslandCollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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

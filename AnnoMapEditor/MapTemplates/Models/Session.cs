using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class Session : ObservableBase
    {
        public ObservableCollection<MapElement> Elements { get; } = new();

        public Vector2 Size 
        { 
            get => _size;
            private set => SetProperty(ref _size, value, dependendProperties: new[] { nameof(MapSizeText) }); 
        }
        private Vector2 _size = Vector2.Zero;

        public Rect2 PlayableArea
        {
            get => _playableAea;
            private set => SetProperty(ref _playableAea, value, dependendProperties: new[] { nameof(MapSizeText) });
        }
        private Rect2 _playableAea = new();

        public Region Region 
        { 
            get => _region;
            set => SetProperty(ref _region, value);
        }
        private Region _region;

        public bool ResizingInProgress
        {
            get => _resizingInProgress;
            set => SetProperty(ref _resizingInProgress, value);
        }
        private bool _resizingInProgress = false;

        private MapTemplateDocument _template = new();

        public event EventHandler<SessionResizeEventArgs>? MapSizeConfigChanged;
        public event EventHandler? MapSizeConfigCommitted;

        public string MapSizeText => $"Size: {Size.X}, Playable: {PlayableArea.Width}";


        public Session(Region region)
        {
            _region = region;
            _template = new MapTemplateDocument();
        }


        #region loading

        public static async Task<Session?> FromDataArchive(string a7tinfoPath)
        {
            Region region = Region.DetectFromPath(a7tinfoPath);
            Stream? a7tinfoStream = Settings.Instance!.DataArchive.OpenRead(a7tinfoPath);
            return await FromBinaryStreamAsync(region, a7tinfoStream);
        }

        public static async Task<Session?> FromBinaryFileAsync(string a7tinfoPath)
        {
            Region region = Region.DetectFromPath(a7tinfoPath);
            Stream a7tinfoStream = File.OpenRead(a7tinfoPath);
            return await FromBinaryStreamAsync(region, a7tinfoStream);
        }

        public static async Task<Session?> FromBinaryStreamAsync(Region region, Stream? a7tinfoStream)
        {
            var doc = await Serializer.ReadAsync<MapTemplateDocument>(a7tinfoStream);
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, region);
        }

        public static async Task<Session?> FromXmlFileAsync(string a7tinfoPath)
        {
            Region region = Region.DetectFromPath(a7tinfoPath);
            Stream a7tinfoXmlStream = File.OpenRead(a7tinfoPath);
            return await FromXmlStreamAsync(region, a7tinfoXmlStream);
        }

        public static async Task<Session?> FromXmlStreamAsync(Region region, Stream? a7tinfoXmlStream)
        {
            var doc = await Serializer.ReadFromXmlAsync<MapTemplateDocument>(a7tinfoXmlStream);
            if (doc is null)
                return null;

            return FromTemplateDocument(doc, region);
        }

        public static Session? FromTemplateDocument(MapTemplateDocument document, Region region)
        {
            Session session = new(region)
            {
                Size = new Vector2(document.MapTemplate?.Size),
                PlayableArea = new Rect2(document.MapTemplate?.PlayableArea),
                _template = document
            };

            // TODO: Allow empty templates?
            int startingSpotCounter = 0;
            foreach (TemplateElement elementTemplate in document.MapTemplate!.TemplateElement!)
            {
                MapElement element = MapElement.FromTemplate(elementTemplate);

                if (element is StartingSpotElement startingSpot)
                    startingSpot.Index = startingSpotCounter++;

                session.Elements.Add(element);
            }

            // clear links in the original
            if (session._template.MapTemplate is not null)
                session._template.MapTemplate.TemplateElement = null;

            if (session.Size.X == 0)
                return null;

            return session;
        }

        #endregion loading


        public static Session? FromNewMapDimensions(int mapSize, int playableSize, Region region)
        {
            int margin = (mapSize - playableSize) / 2;

            MapTemplateDocument createdTemplateDoc = new()
            {
                MapTemplate = new()
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

            // create starting spots in the default location
            session.Elements.AddRange(CreateNewStartingSpots(mapSize));

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public static List<StartingSpotElement> CreateNewStartingSpots(int mapSize)
        {
            const int SPACING = 32;
            int halfSize = mapSize / 2;

            List<StartingSpotElement> starts = new()
            {
                new()
                {
                    Position = new(halfSize + SPACING, halfSize + SPACING)
                },
                new()
                {
                    Position = new(halfSize + SPACING, halfSize - SPACING) 
                },
                new()
                {
                    Position = new(halfSize - SPACING, halfSize - SPACING) 
                },
                new()
                {
                    Position = new(halfSize - SPACING, halfSize + SPACING) 
                }
            };

            // assign indices
            for (int i = 0; i < starts.Count; ++i)
                starts[i].Index = i;

            return starts;
        }

        public void ResizeSession(int mapSize, (int x1, int y1, int x2, int y2) playableAreaMargins)
        {
            ResizingInProgress = true;

            Vector2 oldMapSize = new(Size);
            Size = new(mapSize, mapSize);

            Vector2 oldPlayableSize = new(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new(new int[] { playableAreaMargins.x1, playableAreaMargins.y1, playableAreaMargins.x2, playableAreaMargins.y2 });

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

            Vector2 oldMapSize = new(Size);
            Size = new(_template.MapTemplate.Size);

            Vector2 oldPlayableSize = new(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new(_template.MapTemplate.PlayableArea);

            ResizingInProgress = false;

            MapSizeConfigChanged?.Invoke(this, new SessionResizeEventArgs(oldMapSize, oldPlayableSize));
            MapSizeConfigCommitted?.Invoke(this, new EventArgs());
        }

        public MapTemplateDocument? ToTemplate(bool writeInitialArea = false)
        {
            if (_template.MapTemplate?.Size is null || _template.MapTemplate?.PlayableArea is null)
                return null;

            _template.MapTemplate.TemplateElement = new List<TemplateElement>(Elements.Select(x => x.ToTemplate()).Where(x => x is not null)!);
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

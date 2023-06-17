using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared;
using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared.AreaManagerData;
using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared.AreaManagerData.GameObjectStructure;
using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using FileDBSerializing.EncodingAwareStrings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class MapTemplate : ObservableBase
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

        public SessionAsset Session 
        { 
            get => _session;
            set => SetProperty(ref _session, value);
        }
        private SessionAsset _session;

        public bool ResizingInProgress
        {
            get => _resizingInProgress;
            set => SetProperty(ref _resizingInProgress, value);
        }
        private bool _resizingInProgress = false;

        private MapTemplateDocument _templateDocument = new();

        public Gamedata? Gamedata { get; init; }

        public event EventHandler<MapTemplateResizeEventArgs>? MapSizeConfigChanged;

        public event EventHandler? MapSizeConfigCommitted;

        public string MapSizeText => $"Size: {Size.X}, Playable: {PlayableArea.Width}";


        public MapTemplate(SessionAsset session, MapTemplateDocument document, Gamedata? gamedata)
        {
            _session = session;
            _size = new Vector2(document.MapTemplate?.Size);
            _playableAea = new Rect2(document.MapTemplate?.PlayableArea);
            _templateDocument = document;
            Gamedata = gamedata;

            // TODO: Allow empty templates?
            int startingSpotCounter = 0;
            foreach (TemplateElement elementTemplate in document.MapTemplate!.TemplateElement!)
            {
                MapTemplateElement element = MapTemplateElement.FromTemplate(elementTemplate);

                if (element is StartingSpotElement startingSpot)
                    startingSpot.Index = startingSpotCounter++;

                Elements.Add(element);
            }

            // add GameObjects and EditorObjects
            if (gamedata?.GameSessionManager?.AreaManagerData?.Count > 0)
            {
                if (gamedata.GameSessionManager.AreaManagerData.Count != 1)
                    throw new ArgumentException($"Expected 1 AreaManagerData in .a7t GameSessionManager but got {gamedata.GameSessionManager.AreaManagerData.Count}.");

                AreaManagerDataItem areaManagerData = gamedata.GameSessionManager.AreaManagerData[0].Item2;
                areaManagerData.DecompressData();
                DataContent areaManagerContent = areaManagerData.GetDecompressedData()
                    ?? throw new Exception("Could not decompress AreaManagerDataItem.");

                // load labels for GameObjects and EditorObjects
                Dictionary<long, UTF8String> gameObjectLabelMap = new();
                List<Tuple<UTF8String, long>>? gameObjectLabels = gamedata.GameSessionManager?.GameObjectManager?.GameObjectLabelMap;
                if (gameObjectLabels != null)
                {
                    foreach (Tuple<UTF8String, long> entry in gameObjectLabels)
                        gameObjectLabelMap.Add(entry.Item2, entry.Item1);
                }

                // load GameObjects
                foreach (GameObject gameObject in areaManagerContent.AreaObjectManager!.GameObject!.objects!)
                    AddGameObject(gameObject, gameObjectLabelMap);

                // load EditorObjects
                foreach (GameObject editorObject in areaManagerContent.AreaObjectManager!.EditorObject!.objects!)
                    AddGameObject(editorObject, gameObjectLabelMap);
            }

            // clear links in the original
            if (_templateDocument.MapTemplate is not null)
                _templateDocument.MapTemplate.TemplateElement = null;
        }

        private void AddGameObject(GameObject gameObject, Dictionary<long, UTF8String> labelMap)
        {
            long gameObjectId = (long)gameObject.ID!;

            labelMap.TryGetValue(gameObjectId, out UTF8String? label);
            DataManager.Instance.AssetRepository.TryGet(gameObject.guid ?? -1, out StandardAsset? asset);

            GameObjectElement element = GameObjectElement.FromGameObject(gameObject, asset, label);

            Elements.Add(element);
        }

        public MapTemplate(int mapSize, int playableSize, SessionAsset session)
        {
            int margin = (mapSize - playableSize) / 2;

            _templateDocument = new()
            {
                MapTemplate = new()
                {
                    Size = new int[] { mapSize, mapSize },
                    PlayableArea = new int[] { margin, margin, playableSize + margin, playableSize + margin },
                    ElementCount = 4
                }
            };

            _session = session;
            _size = new Vector2(_templateDocument.MapTemplate.Size);
            _playableAea = new Rect2(_templateDocument.MapTemplate.PlayableArea);

            // create starting spots in the default location
            Elements.AddRange(CreateNewStartingSpots(mapSize));
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

        public void ResizeMapTemplate(int mapSize, (int x1, int y1, int x2, int y2) playableAreaMargins)
        {
            ResizingInProgress = true;

            Vector2 oldMapSize = new(Size);
            Size = new(mapSize, mapSize);

            Vector2 oldPlayableSize = new(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new(new int[] { playableAreaMargins.x1, playableAreaMargins.y1, playableAreaMargins.x2, playableAreaMargins.y2 });

            MapSizeConfigChanged?.Invoke(this, new MapTemplateResizeEventArgs(oldMapSize, oldPlayableSize));
        }

        public void ResizeAndCommitMapTemplate(int mapSize, (int x1, int y1, int x2, int y2) playableAreaMargins)
        {
            if (_templateDocument.MapTemplate == null)
                throw new InvalidDataException();

            //Commit means write to template
            _templateDocument.MapTemplate.Size = new int[] { mapSize, mapSize };
            _templateDocument.MapTemplate.PlayableArea = new int[] { 
                playableAreaMargins.x1, 
                playableAreaMargins.y1, 
                playableAreaMargins.x2, 
                playableAreaMargins.y2 
            };

            Vector2 oldMapSize = new(Size);
            Size = new(_templateDocument.MapTemplate.Size);

            Vector2 oldPlayableSize = new(PlayableArea.Width, PlayableArea.Height);
            PlayableArea = new(_templateDocument.MapTemplate.PlayableArea);

            ResizingInProgress = false;

            MapSizeConfigChanged?.Invoke(this, new MapTemplateResizeEventArgs(oldMapSize, oldPlayableSize));
            MapSizeConfigCommitted?.Invoke(this, new EventArgs());
        }

        public MapTemplateDocument? ToTemplateDocument(bool writeInitialArea = false)
        {
            if (_templateDocument.MapTemplate?.Size is null || _templateDocument.MapTemplate?.PlayableArea is null)
                return null;

            _templateDocument.MapTemplate.TemplateElement = Elements
                .Where(x => x is MapTemplateElement)
                .Select(x => ((MapTemplateElement) x).ToTemplate())
                .Where(x => x is not null)
                .ToList();
            _templateDocument.MapTemplate.ElementCount = _templateDocument.MapTemplate.TemplateElement.Count;

            if (Session == SessionAsset.NewWorld)
                _templateDocument.MapTemplate.InitialPlayableArea = _templateDocument.MapTemplate.PlayableArea;
            else
                _templateDocument.MapTemplate.InitialPlayableArea = null;

            return _templateDocument;
        }


        public class MapTemplateResizeEventArgs : EventArgs
        {
            public MapTemplateResizeEventArgs(Vector2 oldMapSize, Vector2 oldPlayableSize)
            {
                OldMapSize = new Vector2(oldMapSize);
                OldPlayableSize = new Vector2(oldPlayableSize);
            }

            public Vector2 OldMapSize { get; }
            public Vector2 OldPlayableSize { get; }
        }
    }
}

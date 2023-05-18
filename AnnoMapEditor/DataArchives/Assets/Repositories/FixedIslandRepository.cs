using Anno.FileDBModels.Anno1800.IslandTemplate;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;
using FileDBSerializing;
using FileDBSerializing.ObjectSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class FixedIslandRepository : Repository, INotifyCollectionChanged, IEnumerable<FixedIslandAsset>
    {
        private static readonly Logger<FixedIslandRepository> _logger = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;


        private readonly List<FixedIslandAsset> _islands = new();

        private readonly IDataArchive _dataArchive;


        public FixedIslandRepository(IDataArchive dataArchive)
        {
            _dataArchive = dataArchive;

            LoadAsync();
        }


        protected override Task DoLoad()
        {
            _logger.LogInformation($"Begin loading fixed islands.");

            foreach (string mapFilePath in _dataArchive.Find("*.a7m"))
            {
                // thumbnail
                string thumbnailPath = Path.Combine(
                    Path.GetDirectoryName(mapFilePath)!,
                    "_gamedata",
                    Path.GetFileNameWithoutExtension(mapFilePath),
                    "mapimage.png");
                BitmapImage thumbnail = _dataArchive.TryLoadPng(thumbnailPath)
                    ?? throw new Exception($"Could not load island thumbnail '{thumbnailPath}'.");

                // open a7minfo
                string infoFilePath = mapFilePath + "info";

                IFileDBDocument infoDoc = ReadFileDB(infoFilePath)
                    ?? throw new Exception($"Could not load a7minfo '{infoFilePath}'.");

                FileDBDocumentDeserializer<IslandTemplateDocument> deserializer = new(
                    new FileDBSerializerOptions() { IgnoreMissingProperties = true }
                );
                IslandTemplateDocument islandTemplate = deserializer.GetObjectStructureFromFileDBDocument(infoDoc) 
                    ?? throw new Exception($"Could not interpret a7minfo '{infoFilePath}'.");

                // get the island's size in tiles
                int sizeInTiles = islandTemplate.MapSize?[0] ?? IslandSize.Default.DefaultSizeInTiles;

                // get slots
                // The list of slots is found at <ObjectMetaInfo><SlotObjects> within the
                // a7mfile. It has two kinds of elements.
                //
                // First there are lists with a single item of the following form. They denote the
                // slot group the following slots belong to.
                //   <None>
                //     <value>
                //       <id>{byte[2]}</id>
                //     </value>
                //   </None>.
                //
                // The actual lists of slots look like this:
                //   <None>
                //     <None>
                //       <ObjectId>{long}</ObjectId>
                //       <ObjectGuid>{long}</ObjectGuid>
                //       <Position>{float[3]}</Position>
                //     <None>
                //   <None>
                Dictionary<long, Slot> mineSlots = new();
                if(islandTemplate.ObjectMetaInfo?.SlotObjects is not null)
                {
                    foreach ((ShortIdValueWrapper id, List<ObjectItem> items) in islandTemplate.ObjectMetaInfo.SlotObjects)
                    {
                        short slotGroupId = id?.value?.id ?? 0;
                        foreach (ObjectItem item in items)
                        {
                            if (item?.ObjectId is null || item?.ObjectGuid is null || 
                                item?.Position is null || item?.Position.Length != 3)
                                continue;

                            long index = item.ObjectId.Value;

                            Slot mineSlot = new()
                            {
                                GroupId = slotGroupId,
                                ObjectId = index,
                                ObjectGuid = item.ObjectGuid.Value,
                                //Position is stored as xyz, and we need x and z
                                Position = new((int)item.Position[0], (int)item.Position[2])
                            };

                            mineSlots.Add(index, mineSlot);
                        }
                    }
                }

                FixedIslandAsset fixedIsland = new()
                {
                    FilePath = mapFilePath,
                    SizeInTiles = sizeInTiles,
                    Thumbnail = thumbnail,
                    Slots = mineSlots
                };

                Add(fixedIsland);
            }

            _logger.LogInformation($"Finished loading fixed islands. Loaded {_islands.Count} islands.");

            return Task.CompletedTask;
        }


        public void Add(FixedIslandAsset fixedIsland)
        {
            _islands.Add(fixedIsland);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, fixedIsland));
            _logger.LogInformation($"Added '{fixedIsland.FilePath}'.");
        }

        public FixedIslandAsset GetByFilePath(string filePath)
        {
            return _islands.FirstOrDefault(i => i.FilePath == filePath)
                ?? throw new Exception();
        }

        public bool TryGetByFilePath(string filePath, [NotNullWhen(false)] out FixedIslandAsset? fixedIslandAsset)
        {
            fixedIslandAsset = _islands.FirstOrDefault(i => i.FilePath == filePath);
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return fixedIslandAsset != null;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }


        private int ReadSizeInTiles(string mapFilePath)
        {
            string infoFilePath = mapFilePath + "info";
            
            IFileDBDocument? infoDoc = ReadFileDB(infoFilePath);
            if (infoDoc?.Roots.FirstOrDefault(x => x.Name == "MapSize" && x.NodeType == FileDBNodeType.Attrib) is not Attrib mapSize)
                return 0;

            int sizeInTiles = BitConverter.ToInt32(new ReadOnlySpan<byte>(mapSize.Content, 0, 4));
            return sizeInTiles;
        }

        public IFileDBDocument? ReadFileDB(string mapFilePath)
        {
            using Stream? stream = _dataArchive?.OpenRead(mapFilePath);
            if (stream == null)
            {
                _logger.LogWarning($"Could not read FileDB from '{mapFilePath}'. The file could not be found.");
                return null;
            }

            try
            {
                var Version = VersionDetector.GetCompressionVersion(stream);
                var parser = new DocumentParser(Version);
                IFileDBDocument? doc = parser.LoadFileDBDocument(stream);

                return doc;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not read FileDB from '{mapFilePath}'.", ex);
                return null;
            }
        }

        private BitmapImage? ReadThumbnail(string mapFilePath)
        {
            string thumbnailPath = Path.Combine(
                Path.GetDirectoryName(mapFilePath)!,
                "_gamedata",
                Path.GetFileNameWithoutExtension(mapFilePath),
                "mapimage.png");

            using Stream? stream = _dataArchive?.OpenRead(thumbnailPath);
            if (stream == null)
            {
                _logger.LogWarning($"Could not load island thumbnail from '{mapFilePath}'. The file could not be found.");
                return null;
            }

            BitmapImage thumbnail = new();
            thumbnail.BeginInit();
            thumbnail.StreamSource = stream;
            thumbnail.CacheOption = BitmapCacheOption.OnLoad;
            thumbnail.EndInit();
            thumbnail.Freeze();

            return thumbnail;
        }


        public IEnumerator<FixedIslandAsset> GetEnumerator() => _islands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _islands.GetEnumerator();
    }
}

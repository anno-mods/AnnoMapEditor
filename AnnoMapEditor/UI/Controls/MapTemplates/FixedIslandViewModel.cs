using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Utilities;
using FileDBSerializing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class FixedIslandViewModel : IslandViewModel
    {
        private static readonly int DefaultSizeInTiles = 192;


        private readonly FixedIslandElement _fixedIsland;

        public override string? Label => _fixedIsland.Label;

        public override int SizeInTiles => _sizeInTiles;
        private int _sizeInTiles = DefaultSizeInTiles;

        public override BitmapImage? Thumbnail => _thumbnail;
        private BitmapImage? _thumbnail;

        public override int? ThumbnailRotation => _thumbnailRotation;
        private int? _thumbnailRotation;


        public FixedIslandViewModel(Session session, FixedIslandElement fixedIsland) 
            : base(session, fixedIsland)
        {
            _fixedIsland = fixedIsland;

            UpdateSizeInTilesAsync();
            UpdateThumbnailAsync();
            UpdateThumbnailRotation();

            fixedIsland.PropertyChanged += FixedIsland_PropertyChanged;
        }

        private void FixedIsland_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FixedIslandElement.Label))
                OnPropertyChanged(nameof(Label));

            if (e.PropertyName == nameof(FixedIslandElement.Rotation))
                UpdateThumbnailRotation();

            if (e.PropertyName == nameof(FixedIslandElement.IslandAsset))
            {
                UpdateSizeInTilesAsync();
                UpdateThumbnailAsync();
            }
        }

        private void UpdateThumbnailRotation()
        {
            _thumbnailRotation = _fixedIsland.Rotation * 90;
            OnPropertyChanged(nameof(ThumbnailRotation));
        }

        private void UpdateSizeInTilesAsync()
        {
            Task.Run(async () =>
            {
                string infoPath = _fixedIsland.IslandAsset + "info";
                using Stream? stream = Settings.Instance.DataArchive?.OpenRead(infoPath);

                if (stream != null)
                {
                    var doc = IslandReader.ReadFileDB(stream);

                    if (doc?.Roots.FirstOrDefault(x => x.Name == "MapSize" && x.NodeType == FileDBNodeType.Attrib) is not Attrib mapSize)
                        return;

                    _sizeInTiles = BitConverter.ToInt32(new ReadOnlySpan<byte>(mapSize.Content, 0, 4));
                    OnPropertyChanged(nameof(SizeInTiles));
                }
            });
        }

        private void UpdateThumbnailAsync()
        {
            Task.Run(async () =>
            {
                string thumbnailPath = Path.Combine(
                    Path.GetDirectoryName(_fixedIsland.IslandAsset.FilePath)!,
                    "_gamedata",
                    Path.GetFileNameWithoutExtension(_fixedIsland.IslandAsset.FilePath),
                    "mapimage.png");
                using Stream? stream = Settings.Instance.DataArchive?.OpenRead(thumbnailPath);

                if (stream != null)
                {
                    BitmapImage thumbnail = new();
                    thumbnail.BeginInit();
                    thumbnail.StreamSource = stream;
                    thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnail.EndInit();
                    thumbnail.Freeze();

                    _thumbnail = thumbnail;
                    OnPropertyChanged(nameof(Thumbnail));
                }
            });
        }
    }
}

using AnnoMapEditor.DataArchives.Assets.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    public class IslandAsset : StandardAsset
    {
        public string FilePath { get; init; }

        public BitmapImage? Thumbnail 
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }
        private BitmapImage? _thumbnail;


        public IslandAsset(XElement valuesXml)
            : base(valuesXml)
        {
        }
    }
}

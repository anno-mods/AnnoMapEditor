using AnnoMapEditor.Utilities;
using System;
using System.Windows.Media;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    public abstract class StandardAsset : ObservableBase
    {
        private static readonly string TemplateName = "Standard";


        public long GUID { get; init; }

        public string? Name { get; init; }

        public string? IconFilename { get; init; }

        public ImageSource? Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }
        private ImageSource? _icon;


        protected StandardAsset()
        {

        }

        public StandardAsset(XElement valuesXml)
        {
            XElement standardValues = valuesXml.Element(TemplateName)
                ?? throw new Exception($"XML is not a valid {nameof(StandardAsset)}. Required section '{TemplateName}' not found.");

            // GUID
            string guidStr = standardValues.Element(nameof(GUID))?.Value
                ?? throw new Exception($"XML is not a valid {nameof(StandardAsset)}. Required attribute '{nameof(GUID)}' not found.");
            if (long.TryParse(guidStr, out long guid))
                GUID = guid;
            else
                throw new Exception($"XML is not a valid {nameof(StandardAsset)}. Invalid value '{guidStr}' for attribute '{nameof(GUID)}'.");

            // Name and IconFilename
            Name = standardValues.Element(nameof(Name))?.Value;
            IconFilename = standardValues.Element(nameof(IconFilename))?.Value;
        }
    }
}

using AnnoMapEditor.DataArchives.Assets.Attributes;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class AssetRepository : Repository
    {
        private static readonly Logger<AssetRepository> _logger = new();

        private const string AssetsXmlPath = "data/config/export/main/asset/assets.xml";


        private readonly IDataArchive _dataArchive;

        private readonly Dictionary<string, Func<XElement, StandardAsset>> _deserializers = new();

        private readonly Dictionary<long, StandardAsset> _assets = new();


        public AssetRepository(IDataArchive dataArchive)
        {
            _dataArchive = dataArchive;
        }


        public void RegisterAssetModel<TAsset>()
            where TAsset : StandardAsset
        {
            AssetTemplateAttribute assetTemplateAttribute = typeof(TAsset).GetCustomAttribute<AssetTemplateAttribute>()
                ?? throw new Exception($"Cannot register type '{typeof(TAsset).FullName}' as an asset model, because it lacks the {nameof(AssetTemplateAttribute)}.");
            string templateName = assetTemplateAttribute.TemplateName;

            ConstructorInfo deserializerConstructor = typeof(TAsset).GetConstructor(new[] { typeof(XElement) })
                ?? throw new Exception($"Cannot register type '{typeof(TAsset).FullName}' as an asset model, because it lacks a deserialization constructor.");
            Func<XElement, StandardAsset> deserializer = (x) => (StandardAsset)deserializerConstructor.Invoke(new[] { x });

            _deserializers.Add(templateName, deserializer);
        }


        protected override Task DoLoad()
        {
            _logger.LogInformation($"Begin loading AssetRepository.");

            using Stream assetsXmlStream = _dataArchive.OpenRead(AssetsXmlPath)
                ?? throw new Exception($"Could not locate assets.xml.");
            XDocument assetsXml = XDocument.Load(assetsXmlStream);

            IEnumerable<XElement> assetXmls = assetsXml.Descendants()
                .Where(d => d.Name == "Template" && d.Parent != null)
                .Select(d => d.Parent!)
                .Where(v => v != null);

            foreach (XElement assetXml in assetXmls)
            {
                // get the deserializer and skip unknown templates
                string templateName = assetXml.Element("Template")!.Value;
                if (!_deserializers.TryGetValue(templateName, out Func<XElement, StandardAsset>? deserializer))
                    continue;

                // deserialize
                XElement valuesXml = assetXml.Element("Values")!;
                StandardAsset asset = deserializer(valuesXml);

                _assets.Add(asset.GUID, asset);
            }

            _logger.LogInformation($"Finished loading AssetRepository. Loaded {_assets.Count} assets.");

            return Task.CompletedTask;
        }


        public bool TryGet<TAsset>(long guid, [MaybeNullWhen(false)] TAsset? asset)
            where TAsset : StandardAsset
        {
            if (_assets.TryGetValue(guid, out var result))
            {
                if (result is TAsset castResult)
                {
                    asset = castResult;
                    return true;
                }
                else
                    throw new InvalidCastException();
            }
            else
            {
                asset = null;
                return false;
            }
        }

        public IEnumerable<TAsset> GetAll<TAsset>()
            where TAsset : StandardAsset
        {
            foreach (StandardAsset asset in _assets.Values)
                if (asset is TAsset castAsset)
                    yield return castAsset;
        }
    }
}

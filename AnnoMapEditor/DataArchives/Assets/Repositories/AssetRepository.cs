using AnnoMapEditor.DataArchives.Assets.Attributes;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public abstract class AssetRepository : Repository
    {
        private const string AssetsXmlPath = "data/config/export/main/asset/assets.xml";


        public static AssetRepository<T> Create<T>(IDataArchive dataArchive)
            where T : StandardAsset
        {
            // load assets.xml
            // TODO: only do this once per IDataArchive
            using Stream assetsXmlStream = dataArchive.OpenRead(AssetsXmlPath)
                ?? throw new Exception($"Could not locate assets.xml.");
            XDocument assetsXml = XDocument.Load(assetsXmlStream);

            AssetTemplateAttribute assetTemplateAttribute = typeof(T).GetCustomAttribute<AssetTemplateAttribute>()
                ?? throw new Exception($"Cannot create {nameof(AssetRepository<T>)}, because {typeof(T)} lacks the {nameof(AssetTemplateAttribute)}.");

            ConstructorInfo deserializerConstructor = typeof(T).GetConstructor(new[] { typeof(XElement) })
                ?? throw new Exception($"Cannot create {nameof(AssetRepository<T>)}, because {typeof(T)} lacks a deserialization constructor.");
            Func<XElement, T> deserializer = (x) => (T)deserializerConstructor.Invoke(new[] { x });

            return new AssetRepository<T>(assetTemplateAttribute.TemplateName, deserializer, assetsXml);
        }
    }
}

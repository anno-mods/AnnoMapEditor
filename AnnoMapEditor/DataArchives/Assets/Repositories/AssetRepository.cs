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

        private static Dictionary<Type, AssetRepository> _repositories = new();

        protected static XDocument AssetsXml
        {
            get;
            private set;
        }

        static AssetRepository() 
        {
            // TODO: This must happen after the RDADataArchive has been initialized.
            using Stream assetsXmlStream = Settings.Instance.DataArchive.OpenRead(AssetsXmlPath);
            AssetsXml = XDocument.Load(assetsXmlStream);
        }


        public static AssetRepository<T> Get<T>()
            where T : StandardAsset
        {
            lock (_repositories)
            {
                if (!_repositories.TryGetValue(typeof(T), out AssetRepository? result))
                {
                    AssetTemplateAttribute assetTemplateAttribute = typeof(T).GetCustomAttribute<AssetTemplateAttribute>()
                        ?? throw new Exception($"Cannot create {nameof(AssetRepository<T>)}, because {typeof(T)} lacks the {nameof(AssetTemplateAttribute)}.");

                    ConstructorInfo deserializerConstructor = typeof(T).GetConstructor(new[] { typeof(XElement) })
                        ?? throw new Exception($"Cannot create {nameof(AssetRepository<T>)}, because {typeof(T)} lacks a deserialization constructor.");
                    Func<XElement, T> deserializer = (x) => (T)deserializerConstructor.Invoke(new[] { x });

                    result = new AssetRepository<T>(assetTemplateAttribute.TemplateName, deserializer);
                    _repositories.Add(typeof(T), result);
                }

                return (AssetRepository<T>)result;
            }
        }
    }
}

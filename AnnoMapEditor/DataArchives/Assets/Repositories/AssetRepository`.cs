using AnnoMapEditor.DataArchives.Assets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class AssetRepository<T> : AssetRepository, INotifyCollectionChanged, IEnumerable<T>
        where T : StandardAsset
    {

        private readonly Dictionary<long, T> _assetsByGuid = new();

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        private readonly string _templateName;

        private readonly Func<XElement, T> _deserializer;


        public AssetRepository(string templateName, Func<XElement, T> deserializer)
        {
            _templateName = templateName;
            _deserializer = deserializer;

            LoadAsync();
        }


        protected override Task DoLoad()
        {
            IEnumerable<XElement> assetValuesXmls = AssetsXml.Descendants()
                .Where(d => d.Name == "Template" && d.Value == _templateName && d.Parent != null)
                .Select(d => d.Parent!)
                .Select(p => p.Element("Values")!)
                .Where(v => v != null);

            foreach (XElement assetValueXml in assetValuesXmls)
            {
                Add(_deserializer(assetValueXml));
            }

            return Task.CompletedTask;
        }


        public void Add(T asset)
        {
            if (!_assetsByGuid.ContainsKey(asset.GUID))
            {
                _assetsByGuid.Add(asset.GUID, asset);
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, asset));
            }
            else
                throw new Exception();
        }

        public void AddOrReplace(T asset)
        {
            NotifyCollectionChangedEventArgs e;
            if (_assetsByGuid.TryGetValue(asset.GUID, out T? oldAsset))
                e = new(NotifyCollectionChangedAction.Replace, asset, oldAsset);
            else
                e = new(NotifyCollectionChangedAction.Add, asset);

            _assetsByGuid[asset.GUID] = asset;
            CollectionChanged?.Invoke(this, e);
        }

        public T Get(long guid)
        {
            return _assetsByGuid[guid];
        }

        public bool TryGet(long guid, [NotNullWhen(false)] out T? asset)
        {
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return _assetsByGuid.TryGetValue(guid, out asset);
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }


        public IEnumerator<T> GetEnumerator() => _assetsByGuid.Values.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => _assetsByGuid.Values.GetEnumerator();
    }
}

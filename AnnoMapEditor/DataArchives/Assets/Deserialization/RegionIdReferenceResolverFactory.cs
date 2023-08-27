using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    public class RegionIdReferenceResolverFactory
    {
        private readonly AssetRepository _assetRepository;

        private readonly Logger<RegionIdReferenceResolverFactory> _logger;


        public RegionIdReferenceResolverFactory(AssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
            _logger = new Logger<RegionIdReferenceResolverFactory>();
        }


        public Action<object>? CreateResolver<TAsset>(PropertyInfo referenceProperty)
            where TAsset : StandardAsset
        {
            // determine if this property is a reference to a RegionAsset
            RegionIdReferenceAttribute? referenceAttribute = referenceProperty.GetCustomAttribute<RegionIdReferenceAttribute>();
            if (referenceAttribute == null)
                return null;

            // get the property containing the RegionId of the referenced RegionAsset
            PropertyInfo regionIdProperty = typeof(TAsset).GetProperty(referenceAttribute.RegionIdPropertyName)
                ?? throw new ArgumentException($"Invalid {nameof(RegionIdReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. Could not find property {referenceAttribute.RegionIdPropertyName}.");

            // create the resolver
            Action<object> resolver;
            if (regionIdProperty.PropertyType == typeof(string))
                return CreateSingleResolver<TAsset>(referenceProperty, regionIdProperty);

            else if (regionIdProperty.PropertyType == typeof(IEnumerable<string>))
                return CreateEnumerableResolver<TAsset>(referenceProperty, regionIdProperty);

            else
                throw new ArgumentException($"Invalid {nameof(RegionIdReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. Property {referenceAttribute.RegionIdPropertyName} must either be of type {typeof(string).FullName} or {typeof(IEnumerable<string>).FullName}.");
        }

        private Action<object> CreateSingleResolver<TAsset>(PropertyInfo referenceProperty, PropertyInfo regionIdProperty)
            where TAsset : StandardAsset
        {
            // validate the reference property
            Type referencedType = referenceProperty.PropertyType;
            if (!referencedType.IsAssignableFrom(typeof(RegionAsset)))
                throw new ArgumentException($"Invalid {nameof(RegionIdReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. The type {typeof(RegionAsset).FullName} does not extend the property's type.");

            // create the delegate
            return (asset) =>
            {
                string? regionId = regionIdProperty.GetValue(asset) as string;
                if (regionId != null)
                {
                    RegionAsset? regionAsset = _assetRepository.GetAll<RegionAsset>().FirstOrDefault(r => r.RegionID == regionId);
                    if (regionAsset != null)
                        referenceProperty.SetValue(asset, regionAsset);
                    else
                        _logger.LogWarning($"Could not resolve RegionID reference. No RegionAsset with RegionID {regionId} could be found.");
                }
            };
        }

        private bool IsValidCollectionReferenceProperty(PropertyInfo referenceProperty)
        {
            if (!referenceProperty.PropertyType.IsGenericType)
                return false;

            // validate the reference property
            Type referencedType = referenceProperty.PropertyType.GenericTypeArguments[0];
            if (!referencedType.IsAssignableFrom(typeof(RegionAsset)))
                return false;

            Type enumerableType = typeof(ICollection<>).MakeGenericType(referencedType);
            if (referenceProperty.PropertyType != enumerableType)
                return false;

            return true;
        }

        private Action<object> CreateEnumerableResolver<TAsset>(PropertyInfo referenceProperty, PropertyInfo regionIdProperty)
            where TAsset : StandardAsset
        {
            if (!IsValidCollectionReferenceProperty(referenceProperty))
                throw new ArgumentException($"Invalid {nameof(RegionIdReferenceResolverFactory)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. The property's type does not extend {typeof(ICollection<>)} accepting RegionAssets.");

            // create the delegate
            Type referencedType = referenceProperty.PropertyType.GenericTypeArguments[0];
            Type listType = typeof(List<>).MakeGenericType(referencedType);

            return (asset) =>
            {
                IEnumerable<string>? regionIds = regionIdProperty.GetValue(asset) as IEnumerable<string>;
                if (regionIds != null)
                {
                    List<RegionAsset> regionAssets = _assetRepository.GetAll<RegionAsset>().ToList();
                    IList list = (IList)Activator.CreateInstance(listType)!;
                    foreach (string regionId in regionIds)
                    {
                        RegionAsset? regionAsset = regionAssets.FirstOrDefault(r => r.RegionID == regionId);
                        if (regionAsset != null)
                            list.Add(regionAsset);
                        else
                            _logger.LogWarning($"Could not resolve RegionID reference. No RegionAsset with RegionID {regionId} could be found.");
                    }
                    referenceProperty.SetValue(asset, list);
                }
            };
        }
    }
}
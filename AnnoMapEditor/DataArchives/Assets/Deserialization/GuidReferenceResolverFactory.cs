using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    public class GuidReferenceResolverFactory
    {
        private readonly AssetRepository _assetRepository;

        private readonly Logger<GuidReferenceResolverFactory> _logger;


        public GuidReferenceResolverFactory(AssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
            _logger = new Logger<GuidReferenceResolverFactory>();
        }


        public Action<object>? CreateResolver<TAsset>(PropertyInfo referenceProperty)
            where TAsset : StandardAsset
        {
            // determine if this property is a reference to another asset
            GuidReferenceAttribute? referenceAttribute = referenceProperty.GetCustomAttribute<GuidReferenceAttribute>();
            if (referenceAttribute == null)
                return null;

            // get the property containing the guid of the referenced asset
            PropertyInfo guidProperty = typeof(TAsset).GetProperty(referenceAttribute.GuidPropertyName)
                ?? throw new ArgumentException($"Invalid {nameof(GuidReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. Could not find property {referenceAttribute.GuidPropertyName}.");

            // create the resolver
            if (guidProperty.PropertyType == typeof(long) || guidProperty.PropertyType == typeof(long?))
                return CreateSingleResolver<TAsset>(referenceProperty, guidProperty);

            else if (guidProperty.PropertyType == typeof(IEnumerable<long>))
                return CreateEnumerableResolver<TAsset>(referenceProperty, guidProperty);

            else
                throw new ArgumentException($"Invalid {nameof(GuidReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. Property {referenceAttribute.GuidPropertyName} must either be of type {typeof(long).FullName} or {typeof(IEnumerable<long>).FullName}.");
        }

        private Action<object> CreateSingleResolver<TAsset>(PropertyInfo referenceProperty, PropertyInfo guidProperty)
            where TAsset : StandardAsset
        {
            // validate the reference property
            Type referencedType = referenceProperty.PropertyType;
            if (!typeof(StandardAsset).IsAssignableFrom(referencedType))
                throw new ArgumentException($"Invalid {nameof(GuidReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. The property's type does not extend {typeof(StandardAsset).FullName}.");

            // create the delegate
            return (asset) =>
            {
                long? guid = guidProperty.GetValue(asset) as long?;
                if (guid.HasValue)
                {
                    if (TryGetReferencedAsset(guid.Value, out StandardAsset? referencedAsset, referencedType))
                        referenceProperty.SetValue(asset, referencedAsset);
                }
            };
        }

        private bool IsValidCollectionReferenceProperty(PropertyInfo referenceProperty)
        {
            if (!referenceProperty.PropertyType.IsGenericType)
                return false;

            // validate the reference property
            Type referencedType = referenceProperty.PropertyType.GenericTypeArguments[0];
            if (!typeof(StandardAsset).IsAssignableFrom(referencedType))
                return false;

            Type enumerableType = typeof(ICollection<>).MakeGenericType(referencedType);
            if (referenceProperty.PropertyType != enumerableType)
                return false;

            return true;
        }

        private Action<object> CreateEnumerableResolver<TAsset>(PropertyInfo referenceProperty, PropertyInfo guidProperty)
            where TAsset : StandardAsset
        {
            if (!IsValidCollectionReferenceProperty(referenceProperty))
                throw new ArgumentException($"Invalid {nameof(GuidReferenceAttribute)} on property {typeof(TAsset).FullName}.{referenceProperty.Name}. The property's type does not extend {typeof(ICollection<>)} for matching assets.");

            // create the delegate
            Type referencedType = referenceProperty.PropertyType.GenericTypeArguments[0];
            Type listType = typeof(List<>).MakeGenericType(referencedType);

            return (asset) =>
            {
                IEnumerable<long>? guids = guidProperty.GetValue(asset) as IEnumerable<long>;
                if (guids != null)
                {
                    IList list = (IList)Activator.CreateInstance(listType)!;

                    foreach (long guid in guids)
                    {
                        if (TryGetReferencedAsset(guid, out StandardAsset? referencedAsset, referencedType))
                            list.Add(referencedAsset);
                    }

                    referenceProperty.SetValue(asset, list);
                }
            };
        }

        private bool TryGetReferencedAsset(long guid, [MaybeNullWhen(false)] out StandardAsset? referencedAsset, Type referencedType)
        {
            if (_assetRepository.TryGet(guid, out referencedAsset))
            {
                if (referencedType.IsAssignableFrom(referencedAsset!.GetType()))
                    return true;
                else
                    _logger.LogWarning($"Could not resolve GUID reference to type {referencedType.FullName}. Asset with GUID {guid} has type {referencedAsset.GetType().FullName} instead of expected {referencedType.FullName}.");
            }
            else
                _logger.LogWarning($"Could not resolve GUID reference to type {referencedType.FullName}. No asset with GUID {guid} could be found.");

            return false;
        }
    }
}
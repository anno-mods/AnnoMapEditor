using System;

namespace AnnoMapEditor.DataArchives.Assets.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AssetReferenceAttribute : Attribute
    {
        public string GuidPropertyName { get; init; }


        public AssetReferenceAttribute(string guidPropertyName)
        {
            GuidPropertyName = guidPropertyName;
        }
    }
}
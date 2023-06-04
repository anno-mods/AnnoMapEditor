using System;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RegionIdReferenceAttribute : Attribute
    {
        public string RegionIdPropertyName { get; init; }


        public RegionIdReferenceAttribute(string regionIdPropertyName)
        {
            RegionIdPropertyName = regionIdPropertyName;
        }
    }
}

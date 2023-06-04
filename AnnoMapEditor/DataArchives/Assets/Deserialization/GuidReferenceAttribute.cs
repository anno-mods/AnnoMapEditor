using System;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class GuidReferenceAttribute : Attribute
    {
        public string GuidPropertyName { get; init; }


        public GuidReferenceAttribute(string guidPropertyName)
        {
            GuidPropertyName = guidPropertyName;
        }
    }
}

using System;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GuidReferenceAttribute : Attribute
    {
        public string GuidPropertyName { get; init; }


        public GuidReferenceAttribute(string guidPropertyName)
        {
            GuidPropertyName = guidPropertyName;
        }
    }
}
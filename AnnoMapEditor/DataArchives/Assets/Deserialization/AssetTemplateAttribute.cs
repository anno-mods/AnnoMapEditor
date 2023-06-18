using System;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AssetTemplateAttribute : Attribute
    {
        public string[] TemplateNames { get; init; }


        public AssetTemplateAttribute(params string[] templateNames)
        {
            TemplateNames = templateNames;
        }
    }
}

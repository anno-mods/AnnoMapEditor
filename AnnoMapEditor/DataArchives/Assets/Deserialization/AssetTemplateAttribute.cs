using System;

namespace AnnoMapEditor.DataArchives.Assets.Deserialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AssetTemplateAttribute : Attribute
    {
        public string TemplateName { get; init; }


        public AssetTemplateAttribute(string templateName)
        {
            TemplateName = templateName;
        }
    }
}

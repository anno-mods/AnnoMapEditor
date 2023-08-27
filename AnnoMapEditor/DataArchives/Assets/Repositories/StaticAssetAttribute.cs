using System;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class StaticAssetAttribute : Attribute
    {
        public long GUID { get; init; }


        public StaticAssetAttribute(long guid)
        {
            GUID = guid;
        }
    }
}

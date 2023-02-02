using FileDBSerializing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    internal static class FileDBNodeExtensions
    {
        public static Tag GetTag(this Tag node, string name)
        {
            return node.Children.FirstOrDefault(c => c.Name == name) as Tag
                ?? throw new Exception($"Could not find {nameof(Tag)} '{name}'.");
        }

        public static Attrib GetAttrib(this Tag node, string name)
        {
            return node.Children.FirstOrDefault(c => c.Name == name) as Attrib
                ?? throw new Exception($"Could not find {nameof(Attrib)} '{name}'.");
        }

        public static Tag GetTag(this IFileDBDocument document, string name)
        {
            return document.Roots.FirstOrDefault(c => c.Name == name) as Tag
                ?? throw new Exception($"Could not find {nameof(Tag)} '{name}'.");
        }

        public static Attrib GetAttrib(this IFileDBDocument document, string name)
        {
            return document.Roots.FirstOrDefault(c => c.Name == name) as Attrib
                ?? throw new Exception($"Could not find {nameof(Attrib)} '{name}'.");
        }
    }
}

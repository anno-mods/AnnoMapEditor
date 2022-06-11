using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileDBSerializing;

namespace AnnoMapEditor.External
{
    internal static class IFileDBDocumentExtensions
    {
        public static byte[]? GetBytesFromPath(this IFileDBDocument that, string path)
        {
            Queue<string> parts = new(path.Split('/'));
            if (!parts.Any())
                return null;

            FileDBNode? currentNode = that.Roots.FirstOrDefault(x => x.Name == parts.Dequeue());

            while (parts.Any() && currentNode is Tag tagNode)
            {
                string current = parts.Dequeue();
                currentNode = tagNode.Children.FirstOrDefault(x => x.Name == current);
            }

            if (currentNode is not Attrib attribNode)
                return null;

            return attribNode.Content;
        }

        public static string? GetStringFromPath(this IFileDBDocument that, string path)
        {
            var bytes = GetBytesFromPath(that, path);
            if (bytes is null)
                return null;
            var encoding = new UnicodeEncoding();
            return encoding.GetString(bytes);
        }
    }

    internal static class FileDBNodeExtensions
    {
        public static byte[]? GetBytesFromPath(this FileDBNode that, string path)
        {
            Queue<string> parts = new(path.Split('/'));
            if (!parts.Any())
                return null;

            FileDBNode? currentNode = that;

            while (parts.Any() && currentNode is Tag tagNode)
            {
                string current = parts.Dequeue();
                currentNode = tagNode.Children.FirstOrDefault(x => x.Name == current);
            }

            if (currentNode is not Attrib attribNode)
                return null;

            return attribNode.Content;
        }

        public static string? GetStringFromPath(this FileDBNode that, string path, Encoding encoding)
        {
            var bytes = GetBytesFromPath(that, path);
            if (bytes is null)
                return null;
            var text = encoding.GetString(bytes);
            return text;
        }
    }

    internal static class FileDBReader
    {
        public static async Task<int> ReadTileInSizeFromFileAsync(string mapPath)
        {
            if (Utils.Settings.Instance.DataPath is null)
                return 0;

            string fullFilePath = Path.Combine(Utils.Settings.Instance.DataPath, mapPath) + @"info";
            var doc = await ReadFileDBAsync(fullFilePath);

            if (doc?.Roots.FirstOrDefault(x => x.Name == "MapSize" && x.NodeType == FileDBNodeType.Attrib) is not Attrib mapSize)
                return 0;

            int sizeInTiles = BitConverter.ToInt32(new ReadOnlySpan<byte>(mapSize.Content, 0, 4));
            return sizeInTiles;
        }

        public static async Task<IFileDBDocument?> ReadFileDBAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using FileStream fs = File.OpenRead(filePath);
                    var Version = VersionDetector.GetCompressionVersion(fs);

                    IFileDBDocument? doc = null;
                    if (Version == FileDBDocumentVersion.Version1)
                    {
                        var parser = new DocumentParser<FileDBDocument_V1>();
                        doc = parser.LoadFileDBDocument(fs);
                    }
                    else if (Version == FileDBDocumentVersion.Version2)
                    {
                        var parser = new DocumentParser<FileDBDocument_V2>();
                        doc = parser.LoadFileDBDocument(fs);
                    }

                    return doc;
                }
                catch
                {
                    // TODO error log
                    return null;
                }
            });
        }
    }
}

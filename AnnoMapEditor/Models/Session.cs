using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using FileDBSerializing;
using AnnoMapEditor.External;
using AnnoMapEditor.Utils;

namespace AnnoMapEditor.Models
{
    public class Session
    {
        public List<Island> Islands { get; private set; }
        public Vector2 Size { get; private set; }
        public Rect2 PlayableArea { get; private set; }
        public Region Region { get; set; }

        public Session()
        {
            Islands = new List<Island>();
        }

        private static Region DetectRegionFromPath(string filePath)
        {
            if (filePath.Contains("colony01"))
                return Region.NewWorld;
            else if (filePath.Contains("dlc03") || filePath.Contains("colony_03"))
                return Region.Arctic;
            else if (filePath.Contains("dlc06") || filePath.Contains("colony02"))
                return Region.Enbesa;
            return Region.Moderate;
        }

        public static async Task<Session?> FromA7tinfoAsync(string filePath)
        {
            var doc = await FileDBReader.ReadFileDBAsync(filePath);
            if (doc is null)
                return null;

            Region region = DetectRegionFromPath(filePath);

            var mapTemplate = doc.Roots.FirstOrDefault(x => x.Name == "MapTemplate") as Tag;
            var elements = from element in mapTemplate?.Children
                           where element.Name == "TemplateElement"
                           select (element as Tag)?.Children.FirstOrDefault(x => x.Name == "Element");
            var islands = from node in elements
                          where node is not null
                          select Island.FromFileDBNode(node, region);
            Session session = new()
            {
                Region = region,
                Islands = new List<Island>(await Task.WhenAll(islands)),
                Size = new Vector2(doc.GetBytesFromPath("MapTemplate/Size")),
                PlayableArea = new Rect2(doc.GetBytesFromPath("MapTemplate/PlayableArea"))
            };

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public static async Task<Session?> FromXmlAsync(string filePath)
        {
            XDocument sessionDocument;
            try
            {
                sessionDocument = XDocument.Load(File.OpenRead(filePath));
            }
            catch
            {
                return null;
            }

            if (sessionDocument.Root is null)
                return null;

            Region region = DetectRegionFromPath(filePath);

            var islands = from node in sessionDocument.Descendants("TemplateElement")
                          select Island.FromXElement(node, region);

            Session session = new()
            {
                Region = region,
                Islands = new List<Island>(await Task.WhenAll(islands)),
                Size = new Vector2(sessionDocument.Root.GetValueFromPath("MapTemplate/Size")),
                PlayableArea = new Rect2(sessionDocument.Root.GetValueFromPath("MapTemplate/PlayableArea"))
            };

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public async Task UpdateExternalDataAsync()
        {
            foreach (var island in Islands)
                await island.UpdateExternalDataAsync();
        }
    }
}

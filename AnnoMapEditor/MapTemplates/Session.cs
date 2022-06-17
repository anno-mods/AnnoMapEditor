using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using AnnoMapEditor.MapTemplates.Serializing;

namespace AnnoMapEditor.MapTemplates
{
    public class Session
    {
        public List<Island> Islands { get; private set; }
        public Vector2 Size { get; private set; }
        public Rect2 PlayableArea { get; private set; }
        public Region Region { get; set; }

        private Serializing.A7tinfo.MapTemplateDocument template;

        public string MapSizeText => $"Size: {Size.X}, Playable: {PlayableArea.Width}";

        public Session()
        {
            Size = new Vector2(0, 0);
            Islands = new List<Island>();
            template = new Serializing.A7tinfo.MapTemplateDocument();
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
            return await FromA7tinfoAsync(File.OpenRead(filePath), filePath);
        }

        public static async Task<Session?> FromA7tinfoAsync(Stream? stream, string internalPath)
        {
            var doc = await Serializer.ReadAsync<Serializing.A7tinfo.MapTemplateDocument>(stream);
            if (doc is null)
                return null;

            return await FromTemplateDocument(doc, DetectRegionFromPath(internalPath));
        }

        public static async Task<Session?> FromXmlAsync(string filePath)
        {
            var doc = await Serializer.ReadFromXmlAsync<Serializing.A7tinfo.MapTemplateDocument>(File.OpenRead(filePath));
            if (doc is null)
                return null;

            return await FromTemplateDocument(doc, DetectRegionFromPath(filePath));
        }

        public static async Task<Session?> FromXmlAsync(Stream? stream, string internalPath)
        {
            var doc = await Serializer.ReadFromXmlAsync<Serializing.A7tinfo.MapTemplateDocument>(stream);
            if (doc is null)
                return null;

            return await FromTemplateDocument(doc, DetectRegionFromPath(internalPath));
        }

        public static async Task<Session?> FromTemplateDocument(Serializing.A7tinfo.MapTemplateDocument document, Region region)
        {
            var islands = from element in document.MapTemplate?.TemplateElement
                          where element?.Element is not null
                          select Island.FromSerialized(element, region);
            Session session = new()
            {
                Region = region,
                Islands = new List<Island>(await Task.WhenAll(islands)),
                Size = new Vector2(document.MapTemplate?.Size),
                PlayableArea = new Rect2(document.MapTemplate?.PlayableArea),
                template = document
            };

            // clear links in the original
            if (session.template.MapTemplate is not null)
                session.template.MapTemplate.TemplateElement = null;

            if (session.Size.X == 0)
                return null;

            return session;
        }

        public async Task UpdateExternalDataAsync()
        {
            foreach (var island in Islands)
                await island.UpdateExternalDataAsync();
        }
        public Serializing.A7tinfo.MapTemplateDocumentExport? ToTemplate()
        {
            if (template.MapTemplate is null)
                return null;

            return new Serializing.A7tinfo.MapTemplateDocumentExport()
            {
                MapTemplate = new Serializing.A7tinfo.MapTemplateExport(template.MapTemplate, Islands.Select(x => x.ToTemplate()))
            };
        }

        public async Task SaveToXmlAsync(string filePath)
        {
            var export = ToTemplate();
            if (export is null)
                return;

            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await Serializer.WriteToXmlAsync(export, file);
        }

        public async Task SaveAsync(string filePath)
        {
            var export = ToTemplate();
            if (export is null)
                return;

            var parentPath = Path.GetDirectoryName(filePath);
            if (parentPath is not null)
                Directory.CreateDirectory(parentPath);
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await Serializer.WriteAsync(export, file);
        }
    }
}

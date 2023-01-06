using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Mods.Enums;
using AnnoMapEditor.Mods.Models;
using AnnoMapEditor.Tests.Utils;
using System.Text;
using System.Xml.Linq;

namespace AnnoMapEditor.Tests
{
    static class CollectionExtensions
    {
        public static bool HasNoDuplicates<TSource, TResult>(this IEnumerable<TSource> that, Func<TSource, TResult> selector)
        {
            var asList = that.Select(selector);
            var asSet = new HashSet<TResult>(asList);
            return asList.ToArray().SequenceEqual(asSet.ToArray());
        }
    }

    public class PatchedAssetsFixture
    {
        public readonly Dictionary<MapType, XDocument> Data;

        public PatchedAssetsFixture()
        {
            using Stream assetsXml = File.OpenRead("./TestData/assets.xml");
            Data = new(MapType.All.Select(x =>
            {
                Stream patch = new MemoryStream(Encoding.Unicode.GetBytes(Mod.CreateAssetsModOps(Region.Moderate, MapType.Archipelago, "mods/[Map] test/test.a7t")));
                return new KeyValuePair<MapType, XDocument>(x, XDocument.Load(XmlTest.Patch(assetsXml, patch)!));
            }));
        }
    }

    public class MapTypeData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var type in MapType.All)
                yield return new object[] { type };
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Assets : IClassFixture<PatchedAssetsFixture>
    {
        private readonly PatchedAssetsFixture _assetsFixture;


        public Assets(PatchedAssetsFixture fixture)
        {
            _assetsFixture = fixture;
        }


        [Theory]
        [ClassData(typeof(MapTypeData))]
        public void NotEmpty(MapType mapType)
        {
            var xml = _assetsFixture.Data[mapType];
            var assets = xml.Descendants("Asset");
            Assert.NotEmpty(assets);
        }

        [Theory]
        [ClassData(typeof(MapTypeData))]
        public void IsPatched(MapType mapType)
        {
            var xml = _assetsFixture.Data[mapType];
            var assets = xml.Descendants("Asset");
            Assert.NotEmpty(assets.Where(x => x.GetValueFromPath("Values/MapTemplate/TemplateFilename")?.StartsWith("mods/[Map]") ?? false));
        }

        [Theory]
        [ClassData(typeof(MapTypeData))]
        public void NoDuplicateName(MapType mapType)
        {
            var xml = _assetsFixture.Data[mapType];
            var assets = xml.Descendants("Asset");
            Assert.True(assets.HasNoDuplicates(x => x.GetValueFromPath("Values/Standard/Name") ?? ""));
        }

        [Theory]
        [ClassData(typeof(MapTypeData))]
        public void NoDuplicateTemplateFilename(MapType mapType)
        {
            var xml = _assetsFixture.Data[mapType];
            var assets = xml.Descendants("Asset");
            Assert.True(assets.HasNoDuplicates(x => x.GetValueFromPath("Values/MapTemplate/TemplateFilename") ?? ""));
        }
    }
}
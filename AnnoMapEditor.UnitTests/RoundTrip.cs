using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UnitTests.Utils;

namespace AnnoMapEditor.UnitTests
{
    public class RoundTrip
    {
        [Theory]
        [InlineData("./TestMaps/moderate_c_01.xml")]
        [InlineData("./TestMaps/campaign_chapter03_colony01.xml")]
        [InlineData("./TestMaps/moderate_islandarc_ss_01.xml")]
        [InlineData("./TestMaps/colony02_01.xml")]
        [InlineData("./TestMaps/scenario_02_colony_01.xml")]
        public async Task XmlToA7tinfoToXml(string filePath)
        {
            using Stream inputXml = File.OpenRead(filePath);
            Session? session = await Session.FromXmlAsync(inputXml, filePath);

            Assert.NotNull(session);

            Stream a7tinfo = new MemoryStream();
            var export = session!.ToTemplate();
            Assert.NotNull(export);
            await Serializer.WriteAsync(export!, a7tinfo);

            a7tinfo.Position = 0;
            session = await Session.FromA7tinfoAsync(a7tinfo, filePath);
            Assert.NotNull(session);

            StreamWriter outputXml = new(new MemoryStream());
            var template = session!.ToTemplate();
            Assert.NotNull(template);
            await Serializer.WriteToXmlAsync(template!, outputXml.BaseStream);

            Assert.True(StreamComparer.AreEqual(inputXml, outputXml.BaseStream));
        }
    }
}
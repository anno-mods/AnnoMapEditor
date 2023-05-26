using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.Tests.Utils;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.Tests
{
    public class RoundTrip
    {
        [TheoryWithGameFiles]
        [InlineData("./TestData/moderate_c_01.xml")]
        [InlineData("./TestData/campaign_chapter03_colony01.xml")]
        [InlineData("./TestData/moderate_islandarc_ss_01.xml")]
        [InlineData("./TestData/colony02_01.xml")]
        [InlineData("./TestData/scenario_02_colony_01.xml")]
        public async Task XmlToA7tinfoToXml(string filePath)
        {
            await Settings.Instance.AwaitLoadingAsync();

            using Stream inputXml = File.OpenRead(filePath);
            Region region = Region.DetectFromPath(filePath);
            Session? session = await Session.FromXmlStreamAsync(region, inputXml);

            Assert.NotNull(session);

            var export = session!.ToTemplate();
            Assert.NotNull(export);

            using (Stream a7tinfo = new MemoryStream())
            {
                await Serializer.WriteAsync(export!, a7tinfo);

                a7tinfo.Position = 0;
                session = await Session.FromBinaryStreamAsync(region, a7tinfo);
                Assert.NotNull(session);
            }

            var template = session!.ToTemplate();
            Assert.NotNull(template);

            using (MemoryStream outStream = new MemoryStream())
            {
                await Serializer.WriteToXmlAsync(template!, outStream);

                //Uncomment for debugging:
                //string content = System.Text.Encoding.UTF8.GetString(outStream.ToArray());
                //outStream.Seek(0, SeekOrigin.Begin);

                Assert.True(StreamComparer.AreEqual(inputXml, outStream));
            }
        }
    }
}
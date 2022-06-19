using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * TODO Replace this by a C++/CLI wrapper of the original modloader library.
 */

namespace AnnoMapEditor.Tests.Utils
{
    internal static class XmlTest
    {
        public static Stream? Patch(Stream assets, Stream patch)
        {
            var wd = Directory.GetCurrentDirectory();

            assets.Seek(0, SeekOrigin.Begin);
            patch.Seek(0, SeekOrigin.Begin);

            // prepare input
            Directory.CreateDirectory(Path.Combine(wd, "xmltest"));
            using (var writer = File.Create(Path.Combine(wd, @"xmltest\assets.xml")))
                assets.CopyTo(writer);
            using (var writer = File.Create(Path.Combine(wd, @"xmltest\patch.xml")))
                patch.CopyTo(writer);

            // run xmltest
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = Path.Combine(wd, @"Utils\xmltest.exe"),
                Arguments = $"{Path.Combine(wd, @"xmltest\assets.xml")} {Path.Combine(wd, @"xmltest\patch.xml")}"
            });

            if (process is null)
                return null;

            process.WaitForExit();

            if (!File.Exists(Path.Combine(wd, @"patched.xml")))
                return null;

            return File.OpenRead(Path.Combine(wd, @"patched.xml"));
        }
    }
}

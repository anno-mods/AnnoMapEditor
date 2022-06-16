using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Tests.Utils
{
    internal class StreamComparer
    {
        public static bool AreEqual(Stream expected, Stream actual)
        {
            expected.Position = actual.Position = 0;
            if (expected.Length != actual.Length)
                return false;

            int expectedByte, actualByte;
            while ((expectedByte = expected.ReadByte()) >= 0 && (actualByte = actual.ReadByte()) >= 0)
            {
                if (expectedByte != actualByte)
                    return false;
            }
            return true;
        }
    }
}

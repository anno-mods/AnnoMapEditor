using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Validation
{
    public static class Warnings
    {
        public const string ISLAND_OTHER_REGION = "Islands from other regions than the session's region may not work correctly.";

        public const string TOO_MANY_CONTINENTAL_ISLANDS = "More than 1 continental island in a session may results in visual glitches.";
    }
}

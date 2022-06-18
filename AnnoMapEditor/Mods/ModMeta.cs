using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Mods
{
    class Dlc
    {
        public string? DLC { get; set; }
        public string? Dependant { get; set; }
    }

    class Localized
    {
        public string? Chinese { get; set; }
        public string? English { get; set; }
        public string? French { get; set; }
        public string? German { get; set; }
        public string? Italian { get; set; }
        public string? Japanese { get; set; }
        public string? Korean { get; set; }
        public string? Polish { get; set; }
        public string? Russian { get; set; }
        public string? Spanish { get; set; }
        public string? Taiwanese { get; set; }

        public Localized(string? english = null)
        {
            if (english is not null)
            {
                Chinese = english;
                English = english;
                French = english;
                German = english;
                Italian = english;
                Japanese = english;
                Korean = english;
                Polish = english;
                Russian = english;
                Spanish = english;
                Taiwanese = english;
            }
        }
    }

    class Modinfo
    {
        public string? Version { get; set; }
        public string? ModID { get; set; }
        public string[]? IncompatibleIds { get; set; }
        public string[]? ModDependencies { get; set; }
        public Localized? Category { get; set; }
        public Localized? ModName { get; set; }
        public Localized? Description { get; set; }
        public Localized[]? KnownIssues { get; set; }
        public Dlc[]? DLCDependencies { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorContact { get; set; }
        public string? Image { get; set; }
    }
}

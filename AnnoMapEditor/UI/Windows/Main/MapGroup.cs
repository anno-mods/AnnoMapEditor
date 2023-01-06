using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnnoMapEditor.UI.Windows.Main
{
    public class MapGroup
    {
        public string Name;
        public List<MapInfo> Maps;

        public MapGroup(string name, IEnumerable<string> mapFiles, Regex regex)
        {
            Name = name;
            Maps = mapFiles.Select(x => new MapInfo()
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    string.Join(' ', regex.Match(x).Groups.Values.Skip(1).Select(y => y.Value)).Replace("_", " ")),
                FileName = x
            }).ToList();
        }
    }
}

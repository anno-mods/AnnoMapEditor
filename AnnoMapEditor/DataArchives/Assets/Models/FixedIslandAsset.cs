using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    public class FixedIslandAsset : IslandAsset
    {
        public FixedIslandAsset(XElement valuesXml) 
            : base(valuesXml)
        {
        }
    }
}

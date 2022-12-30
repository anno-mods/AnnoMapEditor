using Anno.FileDBModels.Anno1800.MapTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Models.Elements
{
    public class StartingSpot : MapElement
    {
        public int Index { get; }


        public StartingSpot(int index, Vector2 position)
        {
            Index = index;
            Position = position;
        }


        protected override void ToTemplate(TemplateElement result)
        {
            result.ElementType = (short) MapElementType.StartingSpot;
        }
    }
}

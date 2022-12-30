using Anno.FileDBModels.Anno1800.MapTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Models.Elements
{
    public class RandomIsland : IslandElement
    {
        public IslandSize Size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    _size = value;
                    OnPropertyChanged();
                }
            }
        }
        private IslandSize _size;


        public RandomIsland(IslandSize size)
        {
            _size = size;
        }


        protected override void ToTemplate(TemplateElement result)
        {
            base.ToTemplate(result);

            result.Element!.Size = Size.ElementValue;
            // TODO: Comments! What do Difficulty and Config do? Why do we set them to these values?
            result.Element!.Difficulty = new();
            result.Element!.Config = new()
            {
                Type = new() { id = null },
                Difficulty = new()
            };
        }
    }
}

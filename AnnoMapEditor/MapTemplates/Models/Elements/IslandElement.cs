using Anno.FileDBModels.Anno1800.MapTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Models.Elements
{
    public abstract class IslandElement : MapElement
    {
        public string? Label
        {
            get => _label;
            set
            {
                if (value != _label)
                {
                    _label = value;
                    OnPropertyChanged();
                }
            }
        }
        private string? _label;


        protected override void ToTemplate(TemplateElement result)
        {
            if (Label != null)
            {
                result.Element!.IslandLabel = Label;
            }
        }
    }
}

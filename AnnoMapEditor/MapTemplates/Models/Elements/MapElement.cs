using Anno.FileDBModels.Anno1800.MapTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Models.Elements
{
    public abstract class MapElement : ObservableBase
    {
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (value != _position)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }
        private Vector2 _position = Vector2.Zero;


        public TemplateElement ToTemplate()
        {
            TemplateElement result = new();

            // set common properties
            result.Element = new();
            result.Element.Position = new int[] { Position.Y, Position.X };

            ToTemplate(result);

            return result;
        }

        protected abstract void ToTemplate(TemplateElement result);
    }
}

using Anno.FileDBModels.Anno1800.MapTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates
{
    public abstract class MapElement : ObservableBase
    {
        private Vector2 _position = Vector2.Zero;
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

        public abstract int SizeInTiles 
        {
            get;
        }

        private int _rotation = 0;
        public int Rotation 
        {
            get => _rotation;
            set
            {
                if (value != _rotation)
                {
                    _rotation = value;
                    OnPropertyChanged();
                }
            }
        }


        protected MapElement()
        {
        }


        public abstract TemplateElement ToTemplate();
    }
}

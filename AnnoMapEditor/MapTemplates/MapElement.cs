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
        private Vector2 _position = new Vector2(0, 0);
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

        private int _mapSizeInTiles = 0;
        public int MapSizeInTiles 
        {
            get => _mapSizeInTiles;
            protected set 
            {
                if (value != _mapSizeInTiles)
                {
                    _mapSizeInTiles = value;
                    OnPropertyChanged();
                }
            }
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

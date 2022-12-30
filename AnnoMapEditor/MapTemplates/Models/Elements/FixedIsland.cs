using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Models.Elements
{
    public class FixedIsland : IslandElement
    {
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
        private int _rotation = 0;

        public string MapFilePath
        {
            get => _mapFilePath;
            set
            {
                if (value != _mapFilePath)
                {
                    _mapFilePath = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _mapFilePath;


        public FixedIsland(string mapFilePath, int sizeInTiles)
        {
            _mapFilePath = mapFilePath;
        }
    }
}

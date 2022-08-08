using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Models
{
    public class CreateNewMapViewModel : ViewModelBase
    {
        public int MapSize
        {
            get => _mapSize;
            set
            {
                SetProperty(ref _mapSize, value, new string[] { nameof(PlayableSize), nameof(MaxPlayableSize) });
            }
        }
        private int _mapSize = 2560;
        public int PlayableSize
        {
            get => Math.Min(_playableSize, _mapSize - MIN_MARGIN);
            set
            {
                SetProperty(ref _playableSize, value);
            }
        }
        private int _playableSize = 2160;
        
        public int MaxMapSize { get => 4096; } //Arbitrary Maximum, but larger would be absolutely useless
        public int MinMapSize { get => 704; } //Smallest playable Size with 4 reliable medium starter islands

        public int MaxPlayableSize { get => MapSize - MIN_MARGIN; }
        public int MinPlayableSize { get => MinMapSize - MIN_MARGIN; }

        public bool CanCreate => true;


        
        private const int MIN_MARGIN = 64; //Arbitrary Value
    }
}

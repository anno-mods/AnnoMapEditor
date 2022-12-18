using AnnoMapEditor.MapTemplates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Models
{
    public class SessionPropertiesViewModel : ViewModelBase
    {
        internal Session _session;

        public string MapSizeText
        {
            get => _mapSizeText;
            set
            {
                SetProperty(ref _mapSizeText, value);
            }
        }
        private string _mapSizeText;
        public Region SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (value != _selectedRegion)
                {
                    _selectedRegion = value;
                    _session.Region = value;
                    _session.Update();
                    OnSelectedRegionChanged();
                }
            }
        }
        private Region _selectedRegion;

        public event EventHandler? SelectedRegionChanged;

        private void OnSelectedRegionChanged()
        {
            SelectedRegionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandlePropertyChange(object? sender, PropertyChangedEventArgs args)
        {
            if(args.PropertyName == nameof(MapSize) || args.PropertyName == nameof(Margin))
            {
                if(DragInProgress)
                    _session.ResizeSession(MapSize, PlayableSize);
                else
                    _session.ResizeAndCommitSession(MapSize, PlayableSize);
            }
            else if(args.PropertyName == nameof(DragInProgress))
            {
                if (!DragInProgress)
                    _session.ResizeAndCommitSession(MapSize, PlayableSize);
            }
        }

        public Region[] Regions { get; } = Region.All;

        public int MapSize
        {
            get => _mapSize;
            set
            {
                SetProperty(ref _mapSize, value, new string[] { nameof(Margin), nameof(MaxMargin), nameof(PlayableSize) });
            }
        }
        private int _mapSize = 2560;
        public int Margin
        {
            get => Math.Clamp(_margin, MIN_SINGLE_MARGIN, MapSize / 4);
            set
            {
                SetProperty(ref _margin, value, new string[] { nameof(PlayableSize) });
            }
        }
        private int _margin = MIN_SINGLE_MARGIN;

        public int PlayableSize
        {
            get => MapSize - (2 * Margin);
        }

        public int MaxMapSize { get => 4096; } //Arbitrary Maximum, but larger would be absolutely useless
        public int MinMapSize { get => 704; } //Smallest playable Size with 4 reliable medium starter islands

        public int MaxMargin { get => MapSize/4; }
        public int MinMargin { get => MIN_SINGLE_MARGIN; }

        private const int MIN_SINGLE_MARGIN = 32; //Arbitrary Value

        public bool DragInProgress
        {
            get => _dragInProgress;
            set
            {
                SetProperty(ref _dragInProgress, value);
            }
        }
        private bool _dragInProgress = false;

        public SessionPropertiesViewModel(Session session)
        {
            _session = session;
            _session.MapSizeConfigCommitted += HandleSessionSizeCommitted;

            _selectedRegion = _session.Region;

            MapSize = session.Size.X;
            Margin = (session.Size.X - session.PlayableArea.Width) / 2;

            UpdateMapSizeText();

            this.PropertyChanged += HandlePropertyChange;
        }

        private void UpdateMapSizeText()
        {
            MapSizeText = $"Size: {_session.Size.X}, Playable: {_session.PlayableArea.Width}";
        }

        private void HandleSessionSizeCommitted(object? sender, EventArgs _)
        {
            UpdateMapSizeText();
        }
    }
}

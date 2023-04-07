using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;

namespace AnnoMapEditor.UI.Controls
{
    public class SessionPropertiesViewModel : ObservableBase
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
        private string _mapSizeText = "";
        public Region SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (value != _selectedRegion)
                {
                    _selectedRegion = value;
                    _session.Region = value;
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

        private bool allowSessionUpdate = false;
        private void ResizeSessionValues()
        {
            if (allowSessionUpdate)
            {
                if (DragInProgress)
                    _session.ResizeSession(MapSize, PlayableAreaAsTuple());
                else
                {
                    _session.ResizeAndCommitSession(MapSize, PlayableAreaAsTuple());
                }
            }
        }

        private (int x1, int y1, int x2, int y2) PlayableAreaAsTuple()
        {
            return (_session.PlayableArea.X, _session.PlayableArea.Y, 
                _session.PlayableArea.X + _session.PlayableArea.Width, 
                _session.PlayableArea.Y + _session.PlayableArea.Height);
        }

        public Region[] Regions { get; } = Region.All;

        public int MapSize
        {
            get => _mapSize;
            set
            {
                SetProperty(ref _mapSize, value);
                ResizeSessionValues();
            }
        }
        private int _mapSize = 2560;


        public bool EditPlayableArea
        {
            get => _editPlayableArea;
            set
            {
                SetProperty(ref _editPlayableArea, value);
            }
        }
        private bool _editPlayableArea = false;

        public bool ShowPlayableAreaMargins
        {
            get => _showPlayableAreaMargins;
            set
            {
                SetProperty(ref _showPlayableAreaMargins, value);
            }
        }
        private bool _showPlayableAreaMargins = false;


        public int MaxMapSize { get => 4096; } //Arbitrary Maximum, but larger would be absolutely useless
        public int MinMapSize { get => 704; } //Smallest playable Size with 4 reliable medium starter islands

        public bool DragInProgress
        {
            get => _dragInProgress;
            set
            {
                SetProperty(ref _dragInProgress, value);
                ResizeSessionValues();
            }
        }
        private bool _dragInProgress = false;

        public SessionPropertiesViewModel(Session session)
        {
            _session = session;
            _session.MapSizeConfigCommitted += HandleSessionSizeCommitted;

            _selectedRegion = _session.Region;

            MapSize = session.Size.X;

            allowSessionUpdate = true;

            ResizeSessionValues();

            UpdateMapSizeText();
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

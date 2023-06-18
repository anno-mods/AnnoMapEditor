using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;

namespace AnnoMapEditor.UI.Controls
{
    public class MapTemplatePropertiesViewModel : ObservableBase
    {
        internal MapTemplate _mapTemplate;

        public string MapSizeText
        {
            get => _mapSizeText;
            set
            {
                SetProperty(ref _mapSizeText, value);
            }
        }
        private string _mapSizeText = "";

        public SessionAsset SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (value != _selectedSession)
                {
                    _selectedSession = value;
                    _mapTemplate.Session = value;
                    OnSelectedRegionChanged();
                }
            }
        }
        private SessionAsset _selectedSession;

        public event EventHandler? SelectedRegionChanged;

        private void OnSelectedRegionChanged()
        {
            SelectedRegionChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool allowMapTemplateUpdate = false;

        private void ResizeMapTemplateValues()
        {
            if (allowMapTemplateUpdate)
            {
                if (DragInProgress)
                    _mapTemplate.ResizeMapTemplate(MapSize, PlayableAreaAsTuple());
                else
                {
                    _mapTemplate.ResizeAndCommitMapTemplate(MapSize, PlayableAreaAsTuple());
                }
            }
        }

        private (int x1, int y1, int x2, int y2) PlayableAreaAsTuple()
        {
            return (_mapTemplate.PlayableArea.X, _mapTemplate.PlayableArea.Y, 
                _mapTemplate.PlayableArea.X + _mapTemplate.PlayableArea.Width, 
                _mapTemplate.PlayableArea.Y + _mapTemplate.PlayableArea.Height);
        }

        public IEnumerable<SessionAsset> SupportedSessions { get; } = SessionAsset.SupportedSessions;

        public int MapSize
        {
            get => _mapSize;
            set
            {
                SetProperty(ref _mapSize, value);
                ResizeMapTemplateValues();
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
                ResizeMapTemplateValues();
            }
        }
        private bool _dragInProgress = false;

        public MapTemplatePropertiesViewModel(MapTemplate mapTemplate)
        {
            _mapTemplate = mapTemplate;
            _mapTemplate.MapSizeConfigCommitted += HandleMapTemplateSizeCommitted;

            _selectedSession = _mapTemplate.Session;

            MapSize = mapTemplate.Size.X;

            allowMapTemplateUpdate = true;

            ResizeMapTemplateValues();

            UpdateMapSizeText();
        }


        private void UpdateMapSizeText()
        {
            MapSizeText = $"Size: {_mapTemplate.Size.X}, Playable: {_mapTemplate.PlayableArea.Width}";
        }

        private void HandleMapTemplateSizeCommitted(object? sender, EventArgs _)
        {
            UpdateMapSizeText();
        }
    }
}

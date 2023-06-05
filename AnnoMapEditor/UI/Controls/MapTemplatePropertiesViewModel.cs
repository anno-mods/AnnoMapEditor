using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;

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
        public Region SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                if (value != _selectedRegion)
                {
                    _selectedRegion = value;
                    _mapTemplate.Region = value;
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

        public Region[] Regions { get; } = Region.All;

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

            _selectedRegion = _mapTemplate.Region;

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

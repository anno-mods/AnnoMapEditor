using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.Utilities;
using System;

namespace AnnoMapEditor.UI.Models
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
                    _ = _session.UpdateAsync();
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
                    _session.ResizeSession(MapSize, (AverageMargin + OffsetX, AverageMargin + OffsetY, MapSize - AverageMargin + OffsetX, MapSize - AverageMargin + OffsetY));
                else
                {
                    //Apply Margin and Offset Values even if they were implicitly clamped
                    //This is needed because the clamped values would be used on export too.
                    _averageMargin = AverageMargin;
                    _offsetX = OffsetX;
                    _offsetY = OffsetY;

                    _session.ResizeAndCommitSession(MapSize, (AverageMargin + OffsetX, AverageMargin + OffsetY, MapSize - AverageMargin + OffsetX, MapSize - AverageMargin + OffsetY));
                }
            }
        }

        public Region[] Regions { get; } = Region.All;

        public int MapSize
        {
            get => _mapSize;
            set
            {
                SetProperty(ref _mapSize, 
                    value, 
                    new string[] {
                        nameof(AverageMargin),
                        nameof(OffsetY),
                        nameof(OffsetX),
                        nameof(MinOffset),
                        nameof(MaxOffset),
                        nameof(MaxMargin), 
                        nameof(PlayableSize)
                    });
                ResizeSessionValues();
            }
        }
        private int _mapSize = 2560;


        public bool MarginMode
        {
            get => _marginMode;
            set
            {
                SetProperty(ref _marginMode,
                    value,
                    new string[] {
                        nameof(MarginStepSize),
                        nameof(NonCenteredMarginWarning)
                    });
            }
        }
        private bool _marginMode = false;

        public int MarginStepSize
        {
            get => MarginMode ? 4 : 8;
        }

        public int AverageMargin
        {
            get => Math.Clamp(_averageMargin, MIN_SINGLE_MARGIN, MapSize / 4);
            set
            {
                SetProperty(ref _averageMargin, 
                    value, 
                    new string[] {
                        nameof(NonCenteredMarginWarning),
                        nameof(OffsetX),
                        nameof(OffsetY),
                        nameof(MinOffset),
                        nameof(MaxOffset),
                        nameof(PlayableSize)
                    });

                //Handle Avg. Margin with a half tile
                //(which means an uneven number of tiles for both margin and playable area)
                AlignOffsetsToMargin();

                ResizeSessionValues();
            }
        }
        private int _averageMargin = MIN_SINGLE_MARGIN;

        private void AlignOffsetsToMargin()
        {
            //Non-Tile matching Margin needs an offset of 4.
            if (AverageMargin % 8 == 4)
            {
                if (OffsetX % 8 == 0)
                {
                    OffsetX += 4;
                }
                if (OffsetY % 8 == 0)
                {
                    OffsetY += 4;
                }
            }
            else if (AverageMargin % 8 == 0)
            {
                if (Math.Abs(OffsetX % 8) == 4)
                {
                    OffsetX -= 4;
                }
                if (Math.Abs(OffsetY % 8) == 4)
                {
                    OffsetY -= 4;
                }
            }
        }

        public bool NonCenteredMarginWarning
        {
            //For a warning when not all Margins are equal, but we are in MarginMode Centered
            get => IsMarginNonCentered;
        }

        public bool IsMarginNonCentered
        {
            get => OffsetY != 0 || OffsetX != 0;
        }

        public int OffsetY
        {
            get => Math.Clamp(_offsetY, MinOffset, MaxOffset);
            set
            {
                SetProperty(ref _offsetY, value, new string[] { nameof(NonCenteredMarginWarning) });
                ResizeSessionValues();
            }
        }
        private int _offsetY = 0;

        public int OffsetX
        {
            get => Math.Clamp(_offsetX, MinOffset, MaxOffset);
            set
            {
                SetProperty(ref _offsetX, value, new string[] { nameof(NonCenteredMarginWarning)});
                ResizeSessionValues();
            }
        }
        private int _offsetX = 0;


        public int PlayableSize
        {
            get => MapSize - (AverageMargin * 2);
        }

        public int MaxMapSize { get => 4096; } //Arbitrary Maximum, but larger would be absolutely useless
        public int MinMapSize { get => 704; } //Smallest playable Size with 4 reliable medium starter islands

        public int MaxMargin { get => MapSize/4; }
        public int MinMargin { get => MIN_SINGLE_MARGIN; }

        public int MinOffset { get => -_averageMargin; }
        public int MaxOffset { get => _averageMargin; }

        private const int MIN_SINGLE_MARGIN = 32; //Arbitrary Value

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
            AverageMargin = (session.Size.X - session.PlayableArea.Width) / 2;

            OffsetX = session.PlayableArea.X - AverageMargin;
            OffsetY = session.PlayableArea.Y - AverageMargin;

            if (IsMarginNonCentered)
            {
                MarginMode = true;
            }

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

        public void Center()
        { 
            OffsetX= 0;
            OffsetY=0;
        }
    }
}

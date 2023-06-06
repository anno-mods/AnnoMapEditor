using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.Utilities;
using System;
using System.Windows;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class PlayableAreaViewModel : DraggingViewModel
    {
        public PlayableAreaViewModel(MapTemplate mapTemplate)
        {
            MapTemplate = mapTemplate;
            
            //Flip X/Y
            PosY = mapTemplate.PlayableArea.Position.X;
            PosX = mapTemplate.PlayableArea.Position.Y;

            //Flip here as well?
            PlayableSize = mapTemplate.PlayableArea.Width;

            DragEnded += OnDragEnded;
            mapTemplate.MapSizeConfigCommitted += OnMapTemplateSizeCommitted;
        }

        private MapTemplate MapTemplate { get; init; }

        private const int MinMargin = 32; //Arbitrary Value
        private static readonly Vector2 MinMarginVector = new Vector2(MinMargin, MinMargin);

        /// <summary>
        /// Whether to show numeric values for the margins around the playable area on the control.
        /// </summary>
        public bool ShowPlayableAreaMargins
        {
            get => _showPlayableAreaMargins;
            set
            {
                SetProperty(ref _showPlayableAreaMargins, value);
            }
        }
        private bool _showPlayableAreaMargins = false;

        public int PosX
        {
            get => _posX;
            set
            {
                SetProperty(ref _posX, value, new string[] { nameof(MaxSize) });
            }
        }
        private int _posX;

        public int PosY
        {
            get => _posY;
            set
            {
                SetProperty(ref _posY, value, new string[] { nameof(MaxSize) });
            }
        }
        private int _posY;

        public int ControlWidth => MapTemplate.Size.X - PosX;
        public int ControlHeight => MapTemplate.Size.Y - PosY;

        /// <summary>
        /// Used for automatic font scaling to avoid clipping.
        /// </summary>
        public double MarginFontSize 
        { 
            get
            {
                const int FONTSIZE_CUTOFF = 880;
                if (PlayableSize >= FONTSIZE_CUTOFF)
                    return 100.0;
                else
                    //100 - 40xNormalized value below cutoff, as Fontsize 60 is safe even with 4-Figure margin
                    return 100.0 - 40.0*(1.0 - ((PlayableSize - MinSize)/(double)(FONTSIZE_CUTOFF - MinSize)));
            } 
        }

        public int MarginLeftTop
        {
            get => _marginLeftTop;
            set
            {
                SetProperty(ref _marginLeftTop, value);
            }
        }
        private int _marginLeftTop;

        public int MarginRightTop
        {
            get => _marginRightTop;
            set
            {
                SetProperty(ref _marginRightTop, value);
            }
        }
        private int _marginRightTop;

        public int PlayableSize
        {
            get => _playableSize;
            set
            {
                SetProperty(ref _playableSize, value, new string[] { nameof(MarginFontSize) });
            }
        }
        private int _playableSize;

        /// <summary>
        /// Maximum Size allowed during resizing.<br/>
        /// Based on Position and minimum Margin.
        /// </summary>
        public int MaxSize => Math.Min(MapTemplate.Size.X - PosX - MinMargin, MapTemplate.Size.Y - PosY - MinMargin);

        /// <summary>
        /// Always 640 -> Fits 4 medium starters exactly.
        /// </summary>
        public int MinSize => 640; //Arbitrary Value


        public bool ResizingInProgress
        {
            get => _resizingInProgress;
            set
            {
                SetProperty(ref _resizingInProgress, value);
                ResizeMapTemplateValues();
            }
        }
        private bool _resizingInProgress = false;


        public override void OnDragged(Point delta)
        {
            //This limits movement so that the minimum Margin is always remaining.
            Vector2 newPosition = new(PosX + (int)delta.X, PosY + (int)delta.Y);
            newPosition = newPosition.Clamp(MinMarginVector, new Vector2(MapTemplate.Size.X - PlayableSize - MinMargin, MapTemplate.Size.Y - PlayableSize - MinMargin));
            PosX = newPosition.X;
            PosY = newPosition.Y;

            ResizeMapTemplateValues();
        }

        private void OnDragEnded(object? sender, DragEndedEventArgs args)
        {
            ResizeMapTemplateValues();
        }

        private bool LocalResizing { get; set; }
        private void ResizeMapTemplateValues()
        {
            LocalResizing = true;
            if (ResizingInProgress || IsDragging)
                MapTemplate.ResizeMapTemplate(MapTemplate.Size.X, (PosX, PosY, PosX + PlayableSize, PosY + PlayableSize));
            else
                MapTemplate.ResizeAndCommitMapTemplate(MapTemplate.Size.X, (PosX, PosY, PosX + PlayableSize, PosY + PlayableSize));

            LocalResizing = false;
        }

        private void OnMapTemplateSizeCommitted(object? sender, EventArgs _)
        {
            //Use this code only when the map size is resized (externally), to adapt the PlayableArea to the MapSize.
            //Everything else about the PlayableArea gets handled internally already.
            if(!LocalResizing)
            {
                OnPropertyChanged(nameof(MaxSize));

                //The Following Min + if could be solved in one line by nesting two Mins, but that would be terrible to read.
                int maxAllowedSizeX = MapTemplate.Size.X - PosX - MinMargin;
                int maxAllowedSizeY = MapTemplate.Size.Y - PosY - MinMargin;
                int maxAllowedSize = Math.Min(maxAllowedSizeX, maxAllowedSizeY);
                if(PlayableSize > maxAllowedSize)
                {
                    if (maxAllowedSize >= MinSize)
                    {
                        PlayableSize = maxAllowedSize;
                    }
                    else
                    {
                        int offsetX = Math.Min(0, MapTemplate.Size.X - (MinSize + PosX + MinMargin));
                        int offsetY = Math.Min(0, MapTemplate.Size.Y - (MinSize + PosY + MinMargin));

                        PlayableSize = MinSize;
                        //+= because offset will already be negative!
                        PosX += offsetX;
                        PosY += offsetY;
                    }
                }
                ResizeMapTemplateValues();
            }
        }
    }
}

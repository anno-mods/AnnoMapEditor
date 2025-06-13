using System.Collections.Generic;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls.Toolbar
{
    public class ToolbarService : ObservableBase
    {
        
        public static readonly ToolbarService Instance = new();
        
        public event ToolbarButtonClickedEventHandler? ButtonClicked;

        public void ButtonClick(ToolbarButtonType buttonType)
        {
            ButtonClicked?.Invoke(this, new(buttonType));
        }

        public void ButtonContextClick(ToolbarButtonType buttonType, object? sender)
        {
            ButtonClicked?.Invoke(sender, new(buttonType));
        }

        /**
         * Properties for buttons with states
         */

        public bool ShowLabelsButtonState
        {
            get => _showLabelsButtonState;
            set => SetProperty(ref _showLabelsButtonState, value);
        }
        private bool _showLabelsButtonState = true;

        /**
         * Properties for enabled or disabled buttons
         */

        public bool SelectedIslandActionsEnabled
        {
            get => _selectedIslandActionsEnabled;
            set => SetProperty(ref _selectedIslandActionsEnabled, value);
        }
        private bool _selectedIslandActionsEnabled = false;

        public bool ZoomOutButtonEnabled
        {
            get => _zoomOutButtonEnabled;
            set => SetProperty(ref _zoomOutButtonEnabled, value);
        }
        private bool _zoomOutButtonEnabled = false;
        
    }
    
    public delegate void ToolbarButtonClickedEventHandler(object? sender, ToolbarButtonEventArgs e);

    public class ToolbarButtonEventArgs
    {
        public ToolbarButtonEventArgs(ToolbarButtonType buttonType, bool buttonState = false)
        {
            ButtonType = buttonType;
            ButtonState = buttonState;
        }
        
        public ToolbarButtonType ButtonType { get; init; } 
        public bool ButtonState { get; init; }
    }

    public enum ToolbarButtonType
    {
        Undo,
        ShowUndoPopup,
        Redo,
        NewMap,
        LoadMap,
        ImportMap,
        SaveMap,
        SaveMapAs,
        ExportMap,
        NewIsland,
        DeleteIsland,
        CopyIsland,
        PasteIsland,
        CutIsland,
        ZoomReset,
        ZoomIn,
        ZoomOut,
        ShowLabels,
        OpenHelpWindow,
        OpenSettingsWindow
    }
}
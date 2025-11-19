using System.Collections.Generic;
using System.Collections.ObjectModel;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls.Toolbar
{
    public class ToolbarService : ObservableBase
    {
        public static readonly ToolbarService Instance = new();
        
        public event ToolbarButtonClickedEventHandler? ButtonClicked;

        /**
         * Invoked by a button click in the toolbar. Can also be "misused" to trigger specific events by other parts
         * of the app. E.g. resetting map zoom when a map is loaded or newly created. 
         */
        public void ButtonClick(ToolbarButtonType buttonType)
        {
            ButtonClicked?.Invoke(this, new(buttonType));
        }

        /**
         * Can additionally pass the object it is invoked by. Allows to attach context menus to the button and so on.
         */
        public void ButtonContextClick(ToolbarButtonType buttonType, object? sender)
        {
            ButtonClicked?.Invoke(sender, new(buttonType));
        }

        /********************************************
         * Properties for buttons with states
         ********************************************/

        public bool ShowLabelsButtonState
        {
            get => _showLabelsButtonState;
            set => SetProperty(ref _showLabelsButtonState, value);
        }
        private bool _showLabelsButtonState = true;

        /********************************************
         * Properties for enabled or disabled buttons
         ********************************************/

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
    
    /**
     * Event handler for toolbar button clicks.
     */
    public delegate void ToolbarButtonClickedEventHandler(object? sender, ToolbarButtonEventArgs e);

    /**
     * Event Args for the toolbar button click event
     */
    public class ToolbarButtonEventArgs
    {
        public ToolbarButtonEventArgs(ToolbarButtonType buttonType /*, bool buttonState = false */)
        {
            ButtonType = buttonType;
            // ButtonState = buttonState;
        }
        
        public ToolbarButtonType ButtonType { get; init; } 
        
        // If a button has a state (toggle-button), this should represent the current state at the moment of the press.
        // public bool ButtonState { get; init; }
    }

    /**
     * Enum of all available Toolbar buttons that can be listened to
     */
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
using AnnoMapEditor.Utilities.UndoRedo;

namespace AnnoMapEditor.UI.Controls.Toolbar
{
    public class ToolbarViewModel
    {
        public UndoRedoStack UndoRedoStack => UndoRedoStack.Instance;
        public ToolbarService ToolbarService => ToolbarService.Instance;
        
    }
}
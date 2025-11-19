using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public record IslandLabelStackEntry : IUndoRedoStackEntry
    {
        public IslandLabelStackEntry(IslandElement element, string? oldLabel, string? newLabel)
        {
            _element = element;
            _oldLabel = oldLabel;
            _newLabel = newLabel;
        }
        public ActionType ActionType => ActionType.Label;

        private readonly IslandElement _element;
        private readonly string? _oldLabel;
        private readonly string? _newLabel;
        public void Undo()
        {
            _element.Label = _oldLabel;
        }

        public void Redo()
        {
            _element.Label = _newLabel;
        }
    }
}
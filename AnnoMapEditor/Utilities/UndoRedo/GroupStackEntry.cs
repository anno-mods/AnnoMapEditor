using System.Collections.Generic;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public record GroupStackEntry : IUndoRedoStackEntry
    {
        public GroupStackEntry(List<IUndoRedoStackEntry> group)
        {
            _group = group;
        }
        public ActionType ActionType => ActionType.Group;

        private readonly List<IUndoRedoStackEntry> _group;
        public void Undo()
        {
            // Undo in reverse order to properly revert changes inside group changes
            for (int i = _group.Count - 1; i >= 0; i--)
            {
                _group[i].Undo();
            }
        }

        public void Redo()
        {
            foreach (var stackEntry in _group)
            {
                stackEntry.Redo();
            }
        }
    }
}
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
            foreach (var stackEntry in _group)
            {
                stackEntry.Undo();
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
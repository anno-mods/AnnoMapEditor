using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AnnoMapEditor.Utilities.UndoRedoStack;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public interface IUndoRedoStackEntry
    {
        public ActionType ActionType { get; }
        public abstract void Undo();
        public abstract void Redo();
    }
}

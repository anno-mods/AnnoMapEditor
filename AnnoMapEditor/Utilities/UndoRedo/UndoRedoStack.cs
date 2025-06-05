using AnnoMapEditor.Utilities.UndoRedo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities
{
    public class UndoRedoStack : ObservableBase
    {
        public static UndoRedoStack Instance { get; } = new();

        private readonly Stack<IUndoRedoStackEntry> undoStack = new();
        private readonly Stack<IUndoRedoStackEntry> redoStack = new();

        public bool UndoStackAvailable
        {
            get => _undoStackAvailable;
            set
            {
                _undoStackAvailable = value;
                OnPropertyChanged(nameof(UndoStackAvailable));
            }
        }
        private bool _undoStackAvailable;

        public bool RedoStackAvailable
        {
            get => _redoStaclAvailable;
            set
            {
                _redoStaclAvailable = value;
                OnPropertyChanged(nameof(RedoStackAvailable));
            }
        }
        private bool _redoStaclAvailable;

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var stackEntry = undoStack.Pop();
                stackEntry.Undo();
                redoStack.Push(stackEntry);
            }
            UpdateAvailabilities();
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var stackEntry = redoStack.Pop();
                stackEntry.Redo();
                undoStack.Push(stackEntry);
            }
            UpdateAvailabilities();
        }

        /**
         * <summary>
         * Place a stack entry on the undo stack.
         * The Redo stack will be cleared.
         * </summary>
         * <param name="stackEntry">Stack entry that implements `IStackEntry`, containint information on what to do on Undo and Redo.</param>
         */
        public void Do(IUndoRedoStackEntry stackEntry)
        {
            undoStack.Push(stackEntry);
            redoStack.Clear();
            UpdateAvailabilities();
        }

        public void ClearStacks()
        {
            undoStack.Clear();
            redoStack.Clear();
            UpdateAvailabilities();
        }

        private void UpdateAvailabilities()
        {
            UndoStackAvailable = (undoStack.Count > 0);
            RedoStackAvailable = (redoStack.Count > 0);
        }
    }
}

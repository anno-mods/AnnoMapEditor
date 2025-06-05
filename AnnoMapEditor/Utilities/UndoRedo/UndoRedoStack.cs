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

        public enum ActionType
        {
            MapElementTransform
        }

        private Stack<IStackEntry> UndoStack { get; } = new Stack<IStackEntry>();
        private Stack<IStackEntry> RedoStack { get; } = new Stack<IStackEntry>();

        public interface IStackEntry
        {
            public ActionType ActionType { get; init; } 
            public abstract void Undo();
            public abstract void Redo();
        }

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
            if (UndoStack.Count > 0)
            {
                var stackEntry = UndoStack.Pop();
                stackEntry.Undo();
                RedoStack.Push(stackEntry);
            }
            UpdateAvailabilities();
        }

        public void Redo()
        {
            if (RedoStack.Count > 0)
            {
                var stackEntry = RedoStack.Pop();
                stackEntry.Redo();
                UndoStack.Push(stackEntry);
            }
            UpdateAvailabilities();
        }

        public void Do(IStackEntry stackEntry)
        {
            UndoStack.Push(stackEntry);
            RedoStack.Clear();
            UpdateAvailabilities();
        }

        public void ClearStacks()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            UpdateAvailabilities();
        }

        private void UpdateAvailabilities()
        {
            UndoStackAvailable = (UndoStack.Count > 0);
            RedoStackAvailable = (RedoStack.Count > 0);
        }
    }
}

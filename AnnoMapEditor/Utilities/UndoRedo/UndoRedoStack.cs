using AnnoMapEditor.Utilities.UndoRedo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities
{
    public class UndoRedoStack : ObservableBase
    {
        public UndoRedoStack()
        {
            UndoHistory = new();
        }

        public class HistoryEntry
        {
            public HistoryEntry(string label, int index)
            {
                this.Label = label;
                this.Index = index;
            }

            public string Label { get; init; }
            public int Index { get; init; }
        }

        public static UndoRedoStack Instance { get; } = new();

        private readonly Stack<IUndoRedoStackEntry> undoStack = new();
        private readonly Stack<IUndoRedoStackEntry> redoStack = new();

        public ObservableCollection<HistoryEntry> UndoHistory { get; protected set; }

        public bool UndoStackAvailable
        {
            get => _undoStackAvailable;
            set => SetProperty(ref _undoStackAvailable, value);
        }
        private bool _undoStackAvailable;

        public bool RedoStackAvailable
        {
            get => _redoStaclAvailable;
            set => SetProperty(ref _redoStaclAvailable, value);
        }
        private bool _redoStaclAvailable;

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var stackEntry = undoStack.Pop();
                stackEntry.Undo();
                redoStack.Push(stackEntry);
                UndoHistory.Remove(UndoHistory.Last());
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
                UndoHistory.Insert(0, new HistoryEntry(stackEntry.ActionType.ToString(), undoStack.Count - 1));
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
            UndoHistory.Insert(0, new HistoryEntry(stackEntry.ActionType.ToString(), undoStack.Count - 1));
            UpdateAvailabilities();
        }

        public void ClearStacks()
        {
            undoStack.Clear();
            redoStack.Clear();
            UndoHistory.Clear();
            UpdateAvailabilities();
        }

        private void UpdateAvailabilities()
        {
            UndoStackAvailable = (undoStack.Count > 0);
            RedoStackAvailable = (redoStack.Count > 0);
            OnPropertyChanged(nameof(UndoHistory));
        }
    }
}

using AnnoMapEditor.MapTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    record MapElementTransformStackEntry : UndoRedoStack.IStackEntry
    {
        public MapElementTransformStackEntry(MapElement element, Vector2 oldPosition, Vector2 newPosition)
        {
            ActionType = UndoRedoStack.ActionType.MapElementTransform;
            this.oldPosition = new(oldPosition);
            this.newPosition = new(newPosition);
            this.element = element;
        }
        public UndoRedoStack.ActionType ActionType { get; init; }

        private readonly Vector2 oldPosition;
        private readonly Vector2 newPosition;
        private readonly MapElement element;

        public void Redo()
        {
            element.Position = new(newPosition);
        }

        public void Undo()
        {
            element.Position = new(oldPosition);
        }
    }
}

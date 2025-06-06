using AnnoMapEditor.MapTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    record MapElementTransformStackEntry : IUndoRedoStackEntry
    {
        public MapElementTransformStackEntry(
            MapElement element, 
            Vector2 oldPosition, 
            Vector2 newPosition,
            byte? oldRotation = null,
            byte? newRotation = null
        ) {
            _oldPosition = new(oldPosition);
            _newPosition = new(newPosition);
            _oldRotation = oldRotation;
            _newRotation = newRotation;
            _element = element;
        }

        public ActionType ActionType => ActionType.MapElementTransform;

        private readonly Vector2 _oldPosition;
        private readonly Vector2 _newPosition;
        private readonly byte? _oldRotation;
        private readonly byte? _newRotation;
        private readonly MapElement _element;

        public void Redo()
        {
            _element.Position = new(_newPosition);
            if (_element is FixedIslandElement fixedIslandElement && _newRotation != null)
                fixedIslandElement.Rotation = _newRotation;
        }

        public void Undo()
        {
            _element.Position = new(_oldPosition);
            if (_element is FixedIslandElement fixedIslandElement && _oldRotation != null)
                fixedIslandElement.Rotation = _oldRotation;
        }
    }
}

using AnnoMapEditor.MapTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public record IslandRemoveStackEntry : IUndoRedoStackEntry
    {

        public IslandRemoveStackEntry(IslandElement element, MapTemplate mapTemplate)
        {
            _islandElement = element;
            _mapTemplate = mapTemplate;
        }

        private IslandElement _islandElement;
        private MapTemplate _mapTemplate;

        public ActionType ActionType => ActionType.IslandRemove;

        public void Redo()
        {
            _mapTemplate.Elements.Remove(_islandElement);
        }

        public void Undo()
        {
            _mapTemplate.Elements.Add(_islandElement);
        }
    }
}

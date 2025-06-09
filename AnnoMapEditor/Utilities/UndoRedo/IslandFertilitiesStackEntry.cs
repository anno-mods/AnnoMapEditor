using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public record IslandFertilitiesStackEntry : IUndoRedoStackEntry
    {
        public ActionType ActionType => ActionType.IslandFertilities;

        public IslandFertilitiesStackEntry(FixedIslandElement islandElement, bool added, FertilityAsset fertility)
        {
            _islandElement = islandElement;
            _added = added;
            _fertility = fertility;
        }
        
        private readonly FixedIslandElement _islandElement;
        private readonly bool _added;
        private readonly FertilityAsset _fertility;
        public void Undo()
        {
            if (_added)
            {
                _islandElement.Fertilities.Remove(_fertility);
            }
            else
            {
                _islandElement.Fertilities.Add(_fertility);
            }
        }

        public void Redo()
        {
            if (_added)
            {
                _islandElement.Fertilities.Add(_fertility);
            }
            else
            {
                _islandElement.Fertilities.Remove(_fertility);
            }
        }
    }
}
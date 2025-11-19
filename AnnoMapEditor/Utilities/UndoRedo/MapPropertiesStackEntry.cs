using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;

namespace AnnoMapEditor.Utilities.UndoRedo
{
    public record MapPropertiesStackEntry : IUndoRedoStackEntry
    {
        public MapPropertiesStackEntry(
            MapTemplate mapTemplate,
            int? oldSize = null,
            int? newSize = null,
            Rect2? oldPlayableArea = null,
            Rect2? newPlayableArea = null,
            SessionAsset? oldSession = null,
            SessionAsset? newSession = null
        )
        {
            _mapTemplate = mapTemplate;
            _oldSize = oldSize;
            _newSize = newSize;
            _oldPlayableArea = oldPlayableArea;
            _newPlayableArea = newPlayableArea;
            _oldSession = oldSession;
            _newSession = newSession;
        }
        public ActionType ActionType => ActionType.MapProperties;

        private readonly MapTemplate _mapTemplate;
        private readonly int? _oldSize;
        private readonly int? _newSize;
        private readonly Rect2? _oldPlayableArea;
        private readonly Rect2? _newPlayableArea;
        private readonly SessionAsset? _oldSession;
        private readonly SessionAsset? _newSession;
        public void Undo()
        {
            if (_oldSize is { } size && _oldPlayableArea is { } playableArea)
                _mapTemplate.RestoreMapSizeConfig(size, playableArea);
            if (_oldSession != null)
                _mapTemplate.Session = _oldSession;
        }

        public void Redo()
        {
            if (_newSize is { } size && _newPlayableArea is { } playableArea)
                _mapTemplate.RestoreMapSizeConfig(size, playableArea);
            if (_newSession != null)
                _mapTemplate.Session = _newSession;
        }
    }
}
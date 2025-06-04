using AnnoMapEditor.DataArchives.Assets.Models;
using FileDBSerializing;

namespace AnnoMapEditor.MapTemplates.Models
{
    public record UndoRedoStackEntry
    {
        public IFileDBDocument doc;
        public long sessionGuid;
        public ActionType actionType;

        public UndoRedoStackEntry(
            IFileDBDocument doc,
            SessionAsset session,
            ActionType actionType
        ) {
            this.doc = doc;
            this.sessionGuid = session.GUID;
            this.actionType = actionType;
        }

        public enum ActionType
        {
            Manual,
            Resize,
            Move
        }

    }
}

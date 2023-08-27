using System;

namespace AnnoMapEditor.UI.Windows.Main
{
    public enum ExportAsModStatus { 
        LoadingRDA,
        ExportOnlyOldWorld,
        SetGamePath,
        AsPlayableMod
    }
    public class ExportStatus
    {
        public bool CanExportAsMod { get; set; }

        [Obsolete]
        public string ExportAsModText { get; set; } = "";

        public ExportAsModStatus TextStatus { get; set; } = ExportAsModStatus.LoadingRDA;
    }
}

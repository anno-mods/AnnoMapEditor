using System;
using System.Windows;

namespace AnnoMapEditor.UI.Windows.Main
{
    public enum DataPathStatusType { 
        LoadingRDA,
        GamePathSet,
        ExtractedRdaPathSet,
        GamePathInvalid
    }

    public enum ConfigureType {
        Change,
        Select
    }

    public class GamePathStatus
    {
        [Obsolete]
        public string Status { get; set; } = string.Empty;

        public DataPathStatusType StatusType { get; set; }

        public string? ToolTip { get; set; }

        public Visibility AutoDetect { get; set; } = Visibility.Collapsed;

        public Visibility Configure { get; set; } = Visibility.Visible;

        [Obsolete]
        public string ConfigureText { get; set; } = string.Empty;
        public ConfigureType ConfigureType { get; set; }
    }
}

using System.Windows;

namespace AnnoMapEditor.UI.Windows.Main
{
    public class DataPathStatus
    {
        public string Status { get; set; } = string.Empty;

        public string? ToolTip { get; set; }

        public Visibility AutoDetect { get; set; } = Visibility.Collapsed;

        public Visibility Configure { get; set; } = Visibility.Visible;

        public string ConfigureText { get; set; } = string.Empty;
    }
}

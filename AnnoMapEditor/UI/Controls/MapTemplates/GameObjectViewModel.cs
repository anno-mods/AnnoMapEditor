using AnnoMapEditor.MapTemplates.Models;
using System.ComponentModel;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class GameObjectViewModel : MapElementViewModel
    {
        static readonly SolidColorBrush White = new(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        static readonly SolidColorBrush DarkGray = new(Color.FromArgb(0xCC, 0xCC, 0xCC, 0xFF));


        public SolidColorBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetProperty(ref _backgroundBrush, value);
        }
        private SolidColorBrush _backgroundBrush = White;

        public string? Label { get; init; }


        public GameObjectViewModel(GameObjectElement element) 
            : base(element)
        {
            UpdateBackground();

            Label = element.Label?.ToString() ?? element.Asset?.Name;

            PropertyChanged += This_PropertyChanged;
        }


        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSelected))
                UpdateBackground();
        }

        private void UpdateBackground()
        {
            if (IsSelected)
                BackgroundBrush = White;

            else
                BackgroundBrush = DarkGray;
        }
    }
}

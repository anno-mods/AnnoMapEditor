using System.Windows;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    /// <summary>
    /// Interaction logic for StartingSpotControl.xaml
    /// </summary>
    public partial class StartingSpotControl : MapElementControl
    {
        public static readonly int MAP_PIN_SIZE = 64;
        public static readonly CornerRadius MAP_PIN_BORDER = new(0, MAP_PIN_SIZE / 2, MAP_PIN_SIZE / 2, MAP_PIN_SIZE / 2);


        public StartingSpotControl()
        {
            InitializeComponent();
        }
    }
}

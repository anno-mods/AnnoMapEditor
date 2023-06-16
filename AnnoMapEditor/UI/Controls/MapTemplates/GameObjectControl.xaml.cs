using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    /// <summary>
    /// Interaction logic for GameObjectControl.xaml
    /// </summary>
    public partial class GameObjectControl : MapElementControl
    {
        public static readonly int MAP_PIN_SIZE = 16;
        public static readonly CornerRadius MAP_PIN_BORDER = new(0, MAP_PIN_SIZE / 2, MAP_PIN_SIZE / 2, MAP_PIN_SIZE / 2);


        public GameObjectControl()
        {
            InitializeComponent();
            Panel.SetZIndex(this, 1);
        }
    }
}

using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Overlays.SelectIsland
{
    /// <summary>
    /// Interaction logic for SelectIslandOverlay.xaml
    /// </summary>
    public partial class SelectIslandOverlay : UserControl
    {
        public SelectIslandOverlay()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;

            DataContextChanged += This_DataContextChanged;
        }


        private void This_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

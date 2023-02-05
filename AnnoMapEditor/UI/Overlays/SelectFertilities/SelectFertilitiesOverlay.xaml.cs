using AnnoMapEditor.UI.Overlays.SelectIsland;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Overlays.SelectFertilities
{
    /// <summary>
    /// Interaction logic for SelectFertilitiesOverlay.xaml
    /// </summary>
    public partial class SelectFertilitiesOverlay : UserControl
    {
        public SelectFertilitiesOverlay()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;

            DataContextChanged += This_DataContextChanged;
        }


        private void This_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectFertilitiesViewModel viewModel = (DataContext as SelectFertilitiesViewModel)!;
            OverlayService.Instance.Close(viewModel);
        }
    }
}

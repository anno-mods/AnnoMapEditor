using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    /// <summary>
    /// Interaction logic for SelectSlotsOverlay.xaml
    /// </summary>
    public partial class SelectSlotsOverlay : UserControl
    {
        public SelectSlotsOverlay()
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
            SelectSlotsViewModel viewModel = (DataContext as SelectSlotsViewModel)!;
            OverlayService.Instance.Close(viewModel);
        }
    }
}

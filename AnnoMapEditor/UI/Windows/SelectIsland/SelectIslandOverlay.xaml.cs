using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Windows.SelectIsland
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            SelectIslandViewModel? viewModel = DataContext as SelectIslandViewModel;
            if (viewModel != null)
            {
                viewModel.Cancel_Clicked();
            }
        }

    }
}

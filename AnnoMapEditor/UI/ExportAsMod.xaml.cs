using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.UI;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI
{
    public partial class ExportAsMod : UserControl
    {
        public ExportAsModViewModel ViewModel { get; init; } = new();

        public ExportAsMod()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            DataContext = ViewModel;
        }

        public void Show(Session session)
        {
            ViewModel.Session = session;
            Visibility = Visibility.Visible;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {   
            await ViewModel.Save();
            Visibility = Visibility.Collapsed;
        }
    }
}

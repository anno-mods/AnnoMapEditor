using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.UI;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace AnnoMapEditor.UI
{
    /// <summary>
    /// Interaction logic for ExportAsMod.xaml
    /// </summary>
    public partial class ExportAsMod : UserControl
    {
        private Session? session;

        public ExportAsModViewModel ViewModel { get; init; } = new();

        public ExportAsMod()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
            DataContext = ViewModel;
        }

        public void Show(Session session)
        {
            this.session = session;
            Visibility = Visibility.Visible;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private async void Export_Click(object sender, RoutedEventArgs e)
        {
            string? modsFolderPath = Settings.Instance.DataPath;
            if (modsFolderPath is not null)
                modsFolderPath = Path.Combine(modsFolderPath, "mods");

            if (!Directory.Exists(modsFolderPath) || session is null)
                return; // TODO handle this somehow

            Mods.Mod mod = new(session);
            await mod.Save(modsFolderPath, ViewModel.ModName, ViewModel.ModID);
            Visibility = Visibility.Collapsed;
        }
    }
}

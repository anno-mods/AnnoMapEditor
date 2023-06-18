using AnnoMapEditor.DataArchives;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI.Windows.Main;
using AnnoMapEditor.Utilities;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Windows.Start
{
    public class StartWindowViewModel : ObservableBase
    {
        private readonly StartWindow _startWindow;

        public DataManager DataManager { get; init; } = DataManager.Instance;

        public Settings Settings { get; init; } = Settings.Instance;


        public StartWindowViewModel(StartWindow startWindow)
        {
            _startWindow = startWindow;

            Settings.PropertyChanged += Settings_PropertyChanged;

            if (Settings.DataPath != null)
                _ = DataManager.TryInitializeAsync(Settings.DataPath);
        }


        private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.DataPath) && Settings.DataPath != null)
                _ = DataManager.TryInitializeAsync(Settings.DataPath);
        }


        public async Task OpenMap(string a7tinfoPath, bool fromArchive = false)
        {
            MapTemplateReader mapTemplateReader = new();
            MapTemplate mapTemplate;

            if (fromArchive)
                mapTemplate = await mapTemplateReader.FromDataArchiveAsync(a7tinfoPath);

            else
                mapTemplate = await mapTemplateReader.FromFileAsync(a7tinfoPath);

            MainWindow mainWindow = new(new MainWindowViewModel(mapTemplate));

            _startWindow.Close();
            mainWindow.Show();
        }

        public void NewMap()
        {
            MainWindow mainWindow = new(new MainWindowViewModel());

            _startWindow.Close();
            mainWindow.Show();
        }

        public void SelectGamePath()
        {
            var picker = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Select your game (i.e. \"Anno 1800/\") folder or a folder."
            };

            if (!string.IsNullOrEmpty(Settings.GamePath))
                picker.SelectedPath = Settings.GamePath;

            if (true == picker.ShowDialog())
                Settings.GamePath = picker.SelectedPath;
        }

        public void SelectDataPath()
        {
            var picker = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Select the folder containing Anno 1800's .rda files or their unpacked contents."
            };

            if (!string.IsNullOrEmpty(Settings.DataPath))
                picker.SelectedPath = Settings.DataPath;

            if (true == picker.ShowDialog())
                Settings.DataPath = picker.SelectedPath;
        }

        public void SelectModsPath()
        {
            var picker = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                UseDescriptionForTitle = true,
                Description = "Select the folder containing mods for Anno 1800."
            };

            if (Settings.ModsPath != null)
                picker.SelectedPath = Settings.ModsPath;

            if (true == picker.ShowDialog())
                Settings.ModsPath = picker.SelectedPath;
        }

        public void PopulateOpenMapMenu(ContextMenu menu)
        {
            menu.Items.Clear();

            MenuItem openMapFile = new() { Header = "Open file..." };
            openMapFile.Click += (_, _) => OpenMapFileDialog();
            menu.Items.Add(openMapFile);
            menu.Items.Add(new Separator());

            foreach (MapGroup group in DataManager.Instance.MapGroupRepository.MapGroups)
            {
                MenuItem groupMenu = new() { Header = group.Name };

                foreach (MapInfo map in group.Maps)
                {
                    MenuItem mapMenu = new() { Header = map.Name, DataContext = map };
                    mapMenu.Click += (_, _) => _ = OpenMap(map.FileName!, true);
                    groupMenu.Items.Add(mapMenu);
                }

                menu.Items.Add(groupMenu);
            }
        }

        public async void OpenMapFileDialog()
        {
            var picker = new OpenFileDialog
            {
                Filter = "Map templates (*.a7tinfo, *.xml)|*.a7tinfo;*.xml"
            };

            if (true == picker.ShowDialog())
            {
                int end = picker.FileName.IndexOf(@"\data\session");
                if (end == -1)
                    end = picker.FileName.IndexOf(@"\data\dlc");
                if (end != -1)
                    Settings.Instance.GamePath = picker.FileName[..end];

                await OpenMap(picker.FileName, false);
            }
        }
    }
}

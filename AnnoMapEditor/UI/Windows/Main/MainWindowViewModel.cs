using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI.Controls;
using AnnoMapEditor.UI.Controls.IslandProperties;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using AnnoMapEditor.Utilities;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AnnoMapEditor.UI.Windows.Main
{
    public class MainWindowViewModel : ObservableBase
    {
        public MapTemplate MapTemplate
        {
            get => _mapTemplate;
            private set
            {
                if (value != _mapTemplate)
                {
                    SetProperty(ref _mapTemplate, value);
                    SelectedIsland = null;

                    MapTemplateProperties = new(value);
                    MapTemplateChecker = new(value);
                }
            }
        }
        private MapTemplate _mapTemplate;

        public MapTemplatePropertiesViewModel MapTemplateProperties 
        { 
            get => _mapTemplateProperties; 
            private set => SetProperty(ref _mapTemplateProperties, value); 
        }
        private MapTemplatePropertiesViewModel _mapTemplateProperties;

        public MapTemplateCheckerViewModel MapTemplateChecker
        {
            get => _mapTemplateChecker;
            private set => SetProperty(ref _mapTemplateChecker, value);
        }
        private MapTemplateCheckerViewModel _mapTemplateChecker;


        public IslandElement? SelectedIsland
        {
            get => _selectedIsland;
            set
            {
                SetProperty(ref _selectedIsland, value);

                if (value == null)
                {
                    SelectedRandomIslandPropertiesViewModel = null;
                    SelectedFixedIslandPropertiesViewModel = null;
                }
                else if (value is RandomIslandElement randomIsland)
                {
                    SelectedRandomIslandPropertiesViewModel = new(randomIsland);
                    SelectedFixedIslandPropertiesViewModel = null;
                }
                else if (value is FixedIslandElement fixedIsland)
                {
                    SelectedRandomIslandPropertiesViewModel = null;
                    SelectedFixedIslandPropertiesViewModel = new(fixedIsland, MapTemplate!.Session.Region);
                }
            }
        }
        private IslandElement? _selectedIsland;

        public RandomIslandPropertiesViewModel? SelectedRandomIslandPropertiesViewModel
        {
            get => _selectedRandomIslandPropertiesViewModel;
            set => SetProperty(ref _selectedRandomIslandPropertiesViewModel, value);
        }
        private RandomIslandPropertiesViewModel? _selectedRandomIslandPropertiesViewModel;

        public FixedIslandPropertiesViewModel? SelectedFixedIslandPropertiesViewModel
        {
            get => _selectedFixedIslandPropertiesViewModel;
            set => SetProperty(ref _selectedFixedIslandPropertiesViewModel, value);
        }
        private FixedIslandPropertiesViewModel? _selectedFixedIslandPropertiesViewModel;

        public SelectIslandViewModel? SelectIslandViewModel
        {
            get => _selectIslandViewModel;
            set
            {
                SetProperty(ref _selectIslandViewModel, value);
                OnPropertyChanged(nameof(ShowOverlay));
            }
        }
        private SelectIslandViewModel? _selectIslandViewModel;

        public bool ShowOverlay => SelectIslandViewModel != null;

        public string? MapTemplateFilePath
        {
            get => _mapTemplateFilePath;
            private set => SetProperty(ref _mapTemplateFilePath, value);
        }
        private string? _mapTemplateFilePath;


        public MainWindowViewModel(MapTemplate mapTemplate)
        {
            MapTemplate = mapTemplate;
        }

        public MainWindowViewModel()
        {
            CreateNewMap();
        }


        public async Task OpenMap(string a7tinfoPath, bool fromArchive = false)
        {
            MapTemplateFilePath = Path.GetFileName(a7tinfoPath);
            MapTemplateReader mapTemplateReader = new();

            if (fromArchive)
                MapTemplate = await mapTemplateReader.FromDataArchiveAsync(a7tinfoPath);

            else
                MapTemplate = await mapTemplateReader.FromFileAsync(a7tinfoPath);
        }

        public void CreateNewMap()
        {
            const int DEFAULT_MAP_SIZE = 2560;
            const int DEFAULT_PLAYABLE_SIZE = 2160;

            MapTemplateFilePath = null;
            MapTemplate = new MapTemplate(DEFAULT_MAP_SIZE, DEFAULT_PLAYABLE_SIZE, SessionAsset.OldWorld);
        }

        public async Task SaveMap(string filePath)
        {
            if (MapTemplate is null)
                return;

            MapTemplateFilePath = Path.GetFileName(filePath);
            MapTemplateWriter mapTemplateWriter = new();

            if (Path.GetExtension(filePath).ToLower() == ".a7tinfo")
                await mapTemplateWriter.WriteA7tinfoAsync(MapTemplate, filePath);
            else
                await mapTemplateWriter.WriteXmlAsync(MapTemplate, filePath);
        }

        public void PopulateOpenMapMenu(ContextMenu menu)
        {
            menu.Items.Clear();

            MenuItem openMapFile = new() { Header = "Open file..." };
            openMapFile.Click += (_, _) => OpenMapFileDialog();
            menu.Items.Add(openMapFile);
            menu.Items.Add(new Separator());

            MenuItem newFile = new() { Header = "New Map file" };
            newFile.Click += (_, _) => CreateNewMap();
            menu.Items.Add(newFile);
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

                await OpenMap(picker.FileName);
            }
        }
    }
}

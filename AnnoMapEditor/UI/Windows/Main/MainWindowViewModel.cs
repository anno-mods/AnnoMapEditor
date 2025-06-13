using System.Collections.Specialized;
using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI.Controls;
using AnnoMapEditor.UI.Controls.IslandProperties;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using AnnoMapEditor.Utilities;
using AnnoMapEditor.Utilities.UndoRedo;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using AnnoMapEditor.UI.Controls.Toolbar;
using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.ExportAsMod;
using MapTemplate = AnnoMapEditor.MapTemplates.Models.MapTemplate;

namespace AnnoMapEditor.UI.Windows.Main
{
    public class MainWindowViewModel : ObservableBase
    {
        public DataManager DataManager => DataManager.Instance;

        public UndoRedoStack UndoRedoStack => UndoRedoStack.Instance;

        public MapTemplate MapTemplate
        {
            get => _mapTemplate;
            private set
            {
                if (value != _mapTemplate)
                {
                    SetProperty(ref _mapTemplate, value);
                    SelectedIsland = null;
                    value.Elements.CollectionChanged += MapTemplate_ElementsChanged;

                    MapTemplateProperties = new(value);
                    MapTemplateChecker = new(value);

                    // A new map has been loaded or created. Clear Stack
                    UndoRedoStack.ClearStacks();
                }
            }
        }
        private MapTemplate _mapTemplate;
        
        public ToolbarViewModel ToolbarViewModel => new ToolbarViewModel();

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

        public string Status { 
            get => _status; 
            set 
            { 
                OnPropertyChanged(nameof(Status)); 
                _status = value;
            } 
        }
        private string _status = "Unknown Status";

        public IslandElement? SelectedIsland
        {
            get => _selectedIsland;
            set
            {
                SetProperty(ref _selectedIsland, value);
                ToolbarService.Instance.SelectedIslandActionsEnabled = value != null;
                SelectedUnifiedIslandPropertiesViewModel = value == null ? null : new UnifiedIslandPropertiesViewModel(value, MapTemplate);
            }
        }
        private IslandElement? _selectedIsland;

        /**
         * Make sure to notice if an element has been remove from the canvas. If it was selected, deselect it.
         */
        private void MapTemplate_ElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Elements list changed!");
            if (SelectedIsland == null && SelectedUnifiedIslandPropertiesViewModel != null)
                SelectedUnifiedIslandPropertiesViewModel = null;
            
            if (SelectedIsland != null && !_mapTemplate.Elements.Contains(SelectedIsland))
            {
                System.Diagnostics.Debug.WriteLine("Selected Island is not contained in the map elements list!");
                SelectedIsland = null;
            }
        }
        
        public UnifiedIslandPropertiesViewModel? SelectedUnifiedIslandPropertiesViewModel
        {
            get => _selectedUnifiedIslandPropertiesViewModel;
            set => SetProperty(ref _selectedUnifiedIslandPropertiesViewModel, value);
        }

        private UnifiedIslandPropertiesViewModel? _selectedUnifiedIslandPropertiesViewModel;

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
            CommonConstructor();
        }

        public MainWindowViewModel()
        {
            CreateNewMap();
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            if (Settings.Instance.DataPath != null)
                Status = $"Game Path: {Settings.Instance.GamePath}";
            
            MapTemplate.ShowLabels = true;
            
            ToolbarService.Instance.ButtonClicked += (sender, e) =>
            {
                switch (e.ButtonType)
                {
                    case ToolbarButtonType.ShowLabels:
                        _mapTemplate.ShowLabels = !_mapTemplate.ShowLabels;
                        break;
                    case ToolbarButtonType.DeleteIsland:
                        DeleteSelectedIsland();
                        break;
                    case ToolbarButtonType.NewMap:
                        CreateNewMap();
                        break;
                    case ToolbarButtonType.ExportMap:
                        OverlayService.Instance.Show(new ExportAsModViewModel(MapTemplate));
                        break;
                    case ToolbarButtonType.LoadMap:
                        OpenMapFileDialog();
                        break;
                    case ToolbarButtonType.ImportMap:
                        if (sender is Button { ContextMenu: not null } button) PopulateOpenMapMenu(button.ContextMenu, true);
                        break;
                    case ToolbarButtonType.SaveMap:
                        SaveMapFileDialog();
                        break;
                    default: break;
                        
                }
            };
        }

        public async Task OpenMap(string a7tinfoPath, bool fromArchive = false)
        {
            MapTemplateFilePath = Path.GetFileName(a7tinfoPath);
            MapTemplateReader mapTemplateReader = new();

            if (fromArchive)
                MapTemplate = await mapTemplateReader.FromDataArchiveAsync(a7tinfoPath);

            else
                MapTemplate = await mapTemplateReader.FromFileAsync(a7tinfoPath);

            // TODO: Find a better solution for this "Hack"
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ZoomReset);
        }

        public void CreateNewMap()
        {
            const int DEFAULT_MAP_SIZE = 2560;
            const int DEFAULT_PLAYABLE_SIZE = 2160;

            MapTemplateFilePath = null;
            MapTemplate = new MapTemplate(DEFAULT_MAP_SIZE, DEFAULT_PLAYABLE_SIZE, SessionAsset.OldWorld); 
            // TODO: Find a better solution for this "Hack"
            ToolbarService.Instance.ButtonClick(ToolbarButtonType.ZoomReset);
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

        public void PopulateOpenMapMenu(ContextMenu menu, bool import_only = false)
        {
            menu.Items.Clear();

            if (!import_only) 
            { 
                MenuItem openMapFile = new() { Header = "Open file..." };
                openMapFile.Click += (_, _) => OpenMapFileDialog();
                menu.Items.Add(openMapFile);
                menu.Items.Add(new Separator());

                MenuItem newFile = new() { Header = "New Map file" };
                newFile.Click += (_, _) => CreateNewMap();
                menu.Items.Add(newFile);
                menu.Items.Add(new Separator());
            }
            else 
            {
                menu.Items.Add(new MenuItem()
                {
                    Header = "Load map template from game...",
                    IsEnabled = false
                }); 
                menu.Items.Add(new Separator());
            }

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

        public async void SaveMapFileDialog()
        {
            var picker = new SaveFileDialog
            {
                DefaultExt = ".a7tinfo",
                Filter = "Map template (*.a7tinfo)|*.a7tinfo|XML map template (*.xml)|*.xml",
                FilterIndex = Path.GetExtension(MapTemplateFilePath)?.ToLower() == ".xml" ? 2 : 1,
                FileName = Path.GetFileName(MapTemplateFilePath),
                OverwritePrompt = true
            };

            if (true == picker.ShowDialog() && picker.FileName is not null)
            {
                await SaveMap(picker.FileName);
            }
        }

        public void Undo()
        {
            UndoRedoStack.Instance.Undo();
        }

        public void Redo()
        {
            UndoRedoStack.Instance.Redo();
        }

        private void DeleteSelectedIsland()
        {
            if (SelectedIsland == null) return;
            UndoRedoStack.Instance.Do(new IslandRemoveStackEntry(SelectedIsland, MapTemplate));
            MapTemplate.Elements.Remove(SelectedIsland);
            // SelectedIsland = null;
            // SelectedUnifiedIslandPropertiesViewModel = null;
        }

        public ICommand UndoCommand => new ActionCommand((_) => Undo());
        public ICommand RedoCommand => new ActionCommand((_) => Redo());
        public ICommand RemoveCommand => new ActionCommand((_) =>
        {
            DeleteSelectedIsland();
        });
    }
}

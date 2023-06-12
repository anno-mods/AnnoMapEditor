﻿using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI.Controls;
using AnnoMapEditor.UI.Controls.IslandProperties;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public GamePathStatus GamePathStatus
        {
            get => _gamePathStatus;
            private set => SetProperty(ref _gamePathStatus, value);
        }
        private GamePathStatus _gamePathStatus = new();

        public List<MapGroup>? Maps
        {
            get => _maps;
            private set => SetProperty(ref _maps, value);
        }
        private List<MapGroup>? _maps;

        public Settings Settings { get; private set; }

        public MainWindowViewModel(MapTemplate mapTemplate)
        {
            MapTemplate = mapTemplate;

            UpdateStatusAndMenus();
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

        private void UpdateStatusAndMenus()
        {
            // TODO: load from assets instead.
            var mapTemplates = DataManager.Instance.DataArchive.Find("*.a7tinfo");

            Maps = new()
            {
                new MapGroup("Campaign", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/campaign")), new(@"\/campaign_([^\/]+)\.")),
                new MapGroup("Moderate, Archipelago", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_archipel")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Atoll", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_atoll")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Corners", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_corners")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Island Arc", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_islandarc")), new(@"\/([^\/]+)\.")),
                new MapGroup("Moderate, Snowflake", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_snowflake")), new(@"\/([^\/]+)\.")),
                new MapGroup("New World, Large", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_l_")), new(@"\/([^\/]+)\.")),
                new MapGroup("New World, Medium", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_m_")), new(@"\/([^\/]+)\.")),
                new MapGroup("New World, Small", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_s_")), new(@"\/([^\/]+)\.")),
                new MapGroup("DLCs", mapTemplates.Where(x => !x.StartsWith(@"data/sessions/")), new(@"data\/([^\/]+)\/.+\/maps\/([^\/]+)"))
                //new MapGroup("Moderate", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate")), new(@"\/([^\/]+)\."))
            };
        }
    }
}

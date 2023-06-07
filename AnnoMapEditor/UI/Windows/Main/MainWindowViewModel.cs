using AnnoMapEditor.DataArchives;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Serializing;
using AnnoMapEditor.UI.Controls;
using AnnoMapEditor.UI.Controls.IslandProperties;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace AnnoMapEditor.UI.Windows.Main
{
    public class MainWindowViewModel : ObservableBase
    {
        public MapTemplate? MapTemplate
        {
            get => _mapTemplate;
            private set
            {
                if (value != _mapTemplate)
                {
                    SetProperty(ref _mapTemplate, value);
                    SelectedIsland = null;

                    if(MapTemplateProperties is not null) 
                        MapTemplateProperties.SelectedRegionChanged -= SelectedRegionChanged;

                    MapTemplateProperties = value is null ? null : new(value);
                    OnPropertyChanged(nameof(MapTemplateProperties));

                    if(MapTemplateProperties is not null)
                        MapTemplateProperties.SelectedRegionChanged += SelectedRegionChanged;

                    MapTemplateChecker = value is null ? null : new(value);
                    OnPropertyChanged(nameof(MapTemplateChecker));
                }
            }
        }
        private MapTemplate? _mapTemplate;
        public MapTemplatePropertiesViewModel? MapTemplateProperties { get; private set; }
        public MapTemplateCheckerViewModel? MapTemplateChecker { get; private set; }

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

        public DataPathStatus DataPathStatus
        {
            get => _dataPathStatus;
            private set => SetProperty(ref _dataPathStatus, value);
        }
        private DataPathStatus _dataPathStatus = new();

        public ExportStatus ExportStatus
        {
            get => _exportStatus;
            private set => SetProperty(ref _exportStatus, value);
        }
        private ExportStatus _exportStatus = new();

        public List<MapGroup>? Maps
        {
            get => _maps;
            private set => SetProperty(ref _maps, value);
        }
        private List<MapGroup>? _maps;

        public Settings Settings { get; private set; }

        public MainWindowViewModel(Settings settings)
        {
            Settings = settings;
            Settings.PropertyChanged += Settings_PropertyChanged;


            UpdateStatusAndMenus();
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.IsLoading))
                UpdateStatusAndMenus();
        }

        private void SelectedRegionChanged(object? sender, EventArgs _)
        {
            UpdateExportStatus();
        }

        public async Task OpenMap(string a7tinfoPath, bool fromArchive = false)
        {
            MapTemplateFilePath = Path.GetFileName(a7tinfoPath);
            MapTemplateReader mapTemplateReader = new();

            if (fromArchive)
                MapTemplate = await mapTemplateReader.FromDataArchiveAsync(a7tinfoPath);

            else
                MapTemplate = await mapTemplateReader.FromFileAsync(a7tinfoPath);

            UpdateExportStatus();
        }

        public void CreateNewMap()
        {
            const int DEFAULT_MAP_SIZE = 2560;
            const int DEFAULT_PLAYABLE_SIZE = 2160;

            MapTemplateFilePath = null;

            MapTemplate = new MapTemplate(DEFAULT_MAP_SIZE, DEFAULT_PLAYABLE_SIZE, SessionAsset.OldWorld);

            UpdateExportStatus();
        }

        public async Task SaveMap(string filePath)
        {
            if (MapTemplate is null)
                return;

            MapTemplateFilePath = Path.GetFileName(filePath);
            MapTemplateWriter mapTemplateWriter = new();

            if (Path.GetExtension(filePath).ToLower() == ".a7tinfo")
                await mapTemplateWriter.ToA7tinfoAsync(MapTemplate, filePath);
            else
                await mapTemplateWriter.ToXmlAsync(MapTemplate, filePath);
        }

        private void UpdateExportStatus()
        {
            if (Settings.IsLoading)
            {
                // still loading
                ExportStatus = new ExportStatus()
                {
                    CanExportAsMod = false,
                    ExportAsModText = "(loading RDA...)"
                };
            }
            else if (Settings.IsValidDataPath)
            {
                bool archiveReady = Settings.DataArchive is RdaDataArchive;

                ExportStatus = new ExportStatus()
                {
                    CanExportAsMod = archiveReady,
                    ExportAsModText = archiveReady ? "As playable mod..." : "As mod: set game path to save"
                };
            }
            else
            {
                ExportStatus = new ExportStatus()
                {
                    ExportAsModText = "As mod: set game path to save",
                    CanExportAsMod = false
                };
            }
        }

        private void UpdateStatusAndMenus()
        {
            if (Settings.IsLoading)
            {
                // still loading
                DataPathStatus = new DataPathStatus()
                {
                    Status = "loading RDA...",
                    ToolTip = "",
                    Configure = Visibility.Collapsed,
                    AutoDetect = Visibility.Collapsed,
                };
            }
            else if (Settings.IsValidDataPath)
            {
                DataPathStatus = new DataPathStatus()
                {
                    Status = Settings.DataArchive is RdaDataArchive ? "Game path set ✔" : "Extracted RDA path set ✔",
                    ToolTip = Settings.DataArchive.DataPath,
                    ConfigureText = "Change...",
                    AutoDetect = Settings.DataArchive is RdaDataArchive ? Visibility.Collapsed : Visibility.Visible,
                };

                Dictionary<string, Regex> templateGroups = new()
                {
                    ["DLCs"] = new(@"data\/(?!=sessions\/)([^\/]+)"),
                    ["Moderate"] = new(@"data\/sessions\/.+moderate"),
                    ["New World"] = new(@"data\/sessions\/.+colony01")
                };

                //load from assets instead.
                var mapTemplates = Settings.DataArchive.Find("*.a7tinfo");

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
            else
            {
                DataPathStatus = new DataPathStatus()
                {
                    Status = "⚠ Game or RDA path not valid.",
                    ToolTip = null,
                    ConfigureText = "Select...",
                    AutoDetect = Visibility.Visible,
                };

                Maps = new();
            }

            UpdateExportStatus();
        }

        private IEnumerable<String>? LoadMapsFromAssets()
        {
            const string assetsXmlPath = "data/config/export/main/asset/assets.xml";

            Stopwatch stopwatch= Stopwatch.StartNew();
            using var assetStream = Settings.DataArchive.OpenRead(assetsXmlPath)
                ?? throw new FileNotFoundException(assetsXmlPath);
            XmlDocument doc = new XmlDocument();
            doc.Load(assetStream);
            var nodes = doc.SelectNodes("//Asset[Template='MapTemplate']/Values/MapTemplate[TemplateRegion]/*[self::TemplateFilename or self::EnlargedTemplateFilename]");
            stopwatch.Stop();
            double i = stopwatch.Elapsed.TotalMilliseconds;
            return nodes?.Cast<XmlNode>().Select(x => Path.ChangeExtension(x.InnerText, "a7tinfo"));
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

using AnnoMapEditor.DataArchives;
using AnnoMapEditor.MapTemplates;

namespace AnnoMapEditor
{
    public class DataPathStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? ToolTip { get; set; }
        public Visibility AutoDetect { get; set; } = Visibility.Collapsed;
        public Visibility Configure { get; set; } = Visibility.Visible;
        public string ConfigureText { get; set; } = string.Empty;
    }

    public class MapGroup
    {
        public string Name;
        public List<MapInfo> Maps;

        public MapGroup(string name, IEnumerable<string> mapFiles, Regex regex)
        {
            Name = name;
            Maps = mapFiles.Select(x => new MapInfo()
            {
                Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    string.Join(' ', regex.Match(x).Groups.Values.Skip(1).Select(y => y.Value)).Replace("_", " ")),
                FileName = x
            }).ToList();
        }
    }

    public class MapInfo
    {
        public string? Name;
        public string? FileName;
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public Session? Session
        {
            get => _session;
            private set => SetProperty(ref _session, value);
        }
        private Session? _session;

        public string? SessionFilePath
        {
            get => _sessionFilePath;
            private set => SetProperty(ref _sessionFilePath, value);
        }
        private string? _sessionFilePath;

        public DataPathStatus DataPathStatus
        {
            get => _dataPathStatus;
            private set => SetProperty(ref _dataPathStatus, value);
        }
        private DataPathStatus _dataPathStatus = new DataPathStatus();

        public List<MapGroup>? Maps
        {
            get => _maps;
            private set => SetProperty(ref _maps, value);
        }
        private List<MapGroup>? _maps;

        public Utils.Settings Settings { get; private set; }

        public MainWindowViewModel(Utils.Settings settings)
        {
            Settings = settings;
            Settings.PropertyChanged += Settings_PropertyChanged;

            // trigger once ourselves
            Settings_PropertyChanged(this, new PropertyChangedEventArgs("IsValidDataPath"));
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsValidDataPath":
                    UpdateStatusAndMenus();
                    break;
                case "DataArchive":
                    UpdateStatusAndMenus();
                    break;
            }
        }

        public async Task OpenMap(string filePath, bool fromArchive = false)
        {
            SessionFilePath = Path.GetFileName(filePath);

            if (fromArchive)
            {
                Stream? fs = Settings?.DataArchive.OpenRead(filePath);
                if (fs is not null)
                    Session = await Session.FromA7tinfoAsync(fs, filePath);
            }
            else
            {
                if (Path.GetExtension(filePath) == ".a7tinfo")
                    Session = await Session.FromA7tinfoAsync(filePath);
                else
                    Session = await Session.FromXmlAsync(filePath);
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
                    AutoDetect = Visibility.Collapsed
                };
            }
            else if (Settings.IsValidDataPath)
            {
                DataPathStatus = new DataPathStatus()
                {
                    Status = Settings.DataArchive is RdaDataArchive ? "Game path set ✔" : "Extracted RDA path set ✔",
                    ToolTip = Settings.DataArchive.Path,
                    ConfigureText = "Change...",
                    AutoDetect = Settings.DataArchive is RdaDataArchive ? Visibility.Collapsed : Visibility.Visible,
                };

                Dictionary<string, Regex> templateGroups = new()
                {
                    ["DLCs"] = new(@"data\/(?!=sessions\/)([^\/]+)"),
                    ["Moderate"] = new(@"data\/sessions\/.+moderate"),
                    ["New World"] = new(@"data\/sessions\/.+colony01")
                };

                var mapTemplates = Settings.DataArchive.Find("**/*.a7tinfo");

                Maps = new()
                {
                    new MapGroup("Campaign", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/campaign")), new(@"\/campaign_([^\/]+)\.")),
                    new MapGroup("Moderate, Archipelago", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_archipel")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Atoll", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_atoll")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Corners", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_corners")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Island Arc", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_islandarc")), new(@"\/([^\/]+)\.")),
                    new MapGroup("Moderate, Snowflake", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/moderate/moderate_snowflake")), new(@"\/([^\/]+)\.")),
                    new MapGroup("New World, Large", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_l")), new(@"\/([^\/]+)\.")),
                    new MapGroup("New World, Medium", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_m")), new(@"\/([^\/]+)\.")),
                    new MapGroup("New World, Small", mapTemplates.Where(x => x.StartsWith(@"data/sessions/maps/pool/colony01/colony01_s")), new(@"\/([^\/]+)\.")),
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
                    AutoDetect = Visibility.Visible
                };

                Maps = new();
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            if (property is null && value is null)
                return;

            if (!(property?.Equals(value) ?? false))
            {
                property = value;
                OnPropertyChanged(propertyName);
            }
        }
        #endregion
    }
}

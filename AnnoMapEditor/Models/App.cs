using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.Models
{
    public class DataPathStatus
    {
        public string Status { get; set; } = string.Empty;
        public string? ToolTip { get; set; }
        public Visibility AutoDetect { get; set; } = Visibility.Collapsed;
        public string ConfigureText { get; set; } = string.Empty;
    }

    public class App : INotifyPropertyChanged
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

        public Utils.Settings Settings { get; private set; }

        public App(Utils.Settings settings)
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
                    if (Settings.IsValidDataPath)
                    {
                        DataPathStatus = new DataPathStatus()
                        {
                            Status = Settings.DataArchive is Utils.RdaDataArchive ? "Game path set ✔" : "Extracted RDA path set ✔",
                            ToolTip = Settings.DataArchive.Path,
                            ConfigureText = "Change...",
                            AutoDetect = Visibility.Collapsed,
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
                    }

                    break;
            }
        }

        public async Task OpenMap(string filePath)
        {
            SessionFilePath = Path.GetFileName(filePath);
            if (Path.GetExtension(filePath) == ".a7tinfo")
                Session = await Session.FromA7tinfoAsync(filePath);
            else
                Session = await Session.FromXmlAsync(filePath);
        }

        #region IPropertyChanged
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

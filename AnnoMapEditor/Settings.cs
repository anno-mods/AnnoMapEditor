using AnnoMapEditor.DataArchives;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor
{
    public class Settings : INotifyPropertyChanged
    {
        public static Settings Instance { get; } = new();

        public IDataArchive DataArchive
        {
            get => _dataArchive;
            private set
            {
                if (_dataArchive is System.IDisposable disposable)
                    disposable.Dispose();
                SetProperty(ref _dataArchive, value);
            }
        }
        private IDataArchive _dataArchive = DataArchives.DataArchive.Open(null);

        public string? DataPath 
        {
            get => _dataArchive.Path;
            set
            {
                if (value != DataPath)
                {
                    LoadDataPath(value);
                    UserSettings.Default.DataPath = DataArchive.Path;
                    UserSettings.Default.Save();
                    OnPropertyChanged(nameof(DataPath));
                    OnPropertyChanged(nameof(IsValidDataPath));
                }
            }
        }

        public bool IsValidDataPath => _dataArchive?.IsValid ?? false;
        public bool IsLoading { get; private set; }

        public Settings()
        {
            DataPath = UserSettings.Default.DataPath;
            if (DataArchive?.IsValid != true)
            {
                // auto detect on start-up if not valid
                DataPath = GetInstallDirFromRegistry();
            }
        }

        public void LoadDataPath(string? path)
        {
            IsLoading = true;

            Task.Run(() => {
                var archive = DataArchives.DataArchive.Open(path);
                IsLoading = false;
                Application.Current.Dispatcher.Invoke(() => DataArchive = archive);
            });
        }

        public static string? GetInstallDirFromRegistry()
        {
            string installDirKey = @"SOFTWARE\WOW6432Node\Ubisoft\Anno 1800";
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(installDirKey);
            return key?.GetValue("InstallDir") as string;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            property = value;
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}

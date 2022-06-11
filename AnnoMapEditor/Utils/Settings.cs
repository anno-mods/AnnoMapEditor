using System.IO;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnnoMapEditor.Utils
{
    public class Settings : INotifyPropertyChanged
    {
        public static Settings Instance = new();

        public string? DataPath 
        {
            get => _dataPath;
            set
            {
                if (value != DataPath)
                {
                    string? validPath = CorrectDataPath(value);
                    _dataPath = validPath ?? value;
                    UserSettings.Default.DataPath = value;
                    UserSettings.Default.Save();
                    IsValidDataPath = validPath is not null;
                    SetProperty(ref _dataPath, value);
                }
            }
        }
        private string? _dataPath;

        public bool IsValidDataPath
        {
            get => _isValidDataPath;
            private set => SetProperty(ref _isValidDataPath, value);
        }
        private bool _isValidDataPath = false;

        public Settings()
        {
            DataPath = UserSettings.Default.DataPath;
        }

        private static string? CorrectDataPath(string? path)
        {
            if (path is null)
                return null;
            if (Directory.Exists(Path.Combine(path, "data/dlc01")))
                return path;
            if (Directory.Exists(Path.Combine(path, "dlc01")))
                return Path.GetDirectoryName(path);
            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = "")
        {
            property = value;
            OnPropertyChanged(propertyName);
        }
    }
}

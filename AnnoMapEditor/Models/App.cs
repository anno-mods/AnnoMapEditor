using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AnnoMapEditor.Models
{
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

        public string DataPathHint
        {
            get => _dataPathHint;
            private set => SetProperty(ref _dataPathHint, value);
        }
        private string _dataPathHint = string.Empty;

        public string DataPathHintIcon
        {
            get => _dataPathHintIcon;
            private set => SetProperty(ref _dataPathHintIcon, value);
        }
        private string _dataPathHintIcon = "❌";

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
                    DataPathHintIcon = Settings.IsValidDataPath ? "✔" : "⚠";
                    DataPathHint = Settings.IsValidDataPath ? "Path looks right." : "Needs contain the folder `data/` with the extracted .rda contents.";
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

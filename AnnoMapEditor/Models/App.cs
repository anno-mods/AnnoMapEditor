using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        public async Task OpenMap(string filePath)
        {
            SessionFilePath = filePath;
            if (System.IO.Path.GetExtension(filePath) == ".a7tinfo")
                Session = await Session.FromA7tinfoAsync(filePath);
            else
                Session = await Session.FromXmlAsync(filePath);
        }

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
    }
}

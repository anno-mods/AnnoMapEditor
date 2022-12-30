using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnnoMapEditor.MapTemplates
{
    public class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

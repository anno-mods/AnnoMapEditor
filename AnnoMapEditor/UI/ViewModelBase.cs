using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnnoMapEditor.UI
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void SetProperty<T>(ref T property, T value, string[]? dependingPropertyNames = null, [CallerMemberName] string propertyName = "")
        {
            if (property is null && value is null)
                return;

            if (!(property?.Equals(value) ?? false))
            {
                property = value;
                OnPropertyChanged(propertyName);
                if (dependingPropertyNames is not null)
                    foreach (var name in dependingPropertyNames)
                        OnPropertyChanged(name);
            }
        }
    }
}

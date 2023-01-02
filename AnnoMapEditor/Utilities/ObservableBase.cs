﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AnnoMapEditor.Utilities
{
    public class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged = delegate { };
        
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        protected void SetProperty<T>(ref T property, T value, string[]? dependendProperties = null, [CallerMemberName] string propertyName = "")
        {
            if (property is null && value is null)
                return;

            if (!(property?.Equals(value) ?? false))
            {
                property = value;
                OnPropertyChanged(propertyName);
                if (dependendProperties is not null)
                    foreach (var name in dependendProperties)
                        OnPropertyChanged(name);
            }
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class ObjectToVisibility : IValueConverter
    {
        public Visibility OnNull { get; set; }

        public ObjectToVisibility()
        {
            OnNull = Visibility.Collapsed;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? OnNull : Visibility.Visible;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ObjectToVisibilityInverse : IValueConverter
    {
        public Visibility OnNull { get; set; }

        public ObjectToVisibilityInverse()
        {
            OnNull = Visibility.Visible;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? OnNull : Visibility.Collapsed;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

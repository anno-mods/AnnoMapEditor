using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class EmptyStringToVisibility : IValueConverter
    {
        public Visibility OnEmpty { get; set; } = Visibility.Collapsed;
        public Visibility OnNotEmpty { get; set; } = Visibility.Visible;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string s)
                return null;
            return string.IsNullOrEmpty(s) ? OnEmpty : OnNotEmpty;

        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
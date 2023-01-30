using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleaNotConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return !boolean;
            else
                throw new ArgumentException();
        }
        
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
                return !boolean;
            else
                throw new ArgumentException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjectToBool : IValueConverter
    {
        public bool OnNull { get; set; }

        public ObjectToBool()
        {
            OnNull = false;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? OnNull : !OnNull;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

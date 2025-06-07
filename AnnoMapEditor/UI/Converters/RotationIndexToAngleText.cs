using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    // [ValueConversion(typeof(byte?), typeof(string))]
    public class RotationIndexToAngleText : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return (value as byte?) switch
            {
                0 => "0°",
                1 => "90°",
                2 => "180°",
                3 => "270°",
                _ => "?"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
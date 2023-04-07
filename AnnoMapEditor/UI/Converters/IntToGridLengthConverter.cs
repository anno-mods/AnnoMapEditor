using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(int), typeof(GridLength))]
    public class IntToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return new GridLength(intValue);
            throw new ArgumentException("Input was not an int.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
                if (gridLength.IsAbsolute)
                    return (int)(gridLength.Value + 0.5);
                else
                    throw new ArgumentException("Can only convert Absolute GridLengths.");
            throw new ArgumentException("Input was not a GridLength.");
        }
    }
}

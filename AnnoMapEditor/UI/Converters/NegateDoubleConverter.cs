using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class NegateDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return -d;
            else
                throw new ArgumentException($"Illegal argument for {nameof(NegateDoubleConverter)}. Argument must be of type double, but got {value.GetType()} instead.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return -d;
            else
                throw new ArgumentException($"Illegal argument for {nameof(NegateDoubleConverter)}. Argument must be of type double, but got {value.GetType()} instead.");
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class BoolToGridLengthConverter : IValueConverter
    {
        public GridLength OnTrue { get; set; }

        public GridLength OnFalse { get; set; }


        public BoolToGridLengthConverter()
        {
            OnTrue = GridLength.Auto;
            OnFalse = new(0);
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? OnTrue : OnFalse;

            throw new ArgumentException("Input was not a bool.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
                return gridLength.Value > 0;

            throw new ArgumentException("Input was not a GridLength.");
        }
    }
}

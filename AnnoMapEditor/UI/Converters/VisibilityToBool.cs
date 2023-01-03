using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(Visibility), typeof(bool))]
    public class VisibilityToBool : IValueConverter
    {
        public bool OnVisible { get; set; }


        public VisibilityToBool()
        {
            OnVisible = true;
        }


        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility visibility)
                return null;
            return visibility switch {
                Visibility.Visible => OnVisible,
                Visibility.Collapsed => !OnVisible,
                Visibility.Hidden => !OnVisible,
                _ => null
            };
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

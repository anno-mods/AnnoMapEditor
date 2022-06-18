using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Controls
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal sealed class BoolToVisibility : IValueConverter
    {
        public Visibility OnTrue { get; set; }
        public Visibility OnFalse { get; set; }

        public BoolToVisibility()
        {
            OnTrue = Visibility.Visible;
            OnFalse = Visibility.Collapsed;
        }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolean)
                return null;
            return boolean ? OnTrue : OnFalse;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Visibility), typeof(bool))]
    internal sealed class VisibilityToBool : IValueConverter
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

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
    class IsActiveToToolbarButtonStyle : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return Application.Current.FindResource(isActive ? "ToolbarButtonActive" : "ToolbarButton") as Style;
            }
            else
                throw new ArgumentException($"Illegal argument for {nameof(NegateDoubleConverter)}. Argument must be of type bool, but got {value.GetType()} instead.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

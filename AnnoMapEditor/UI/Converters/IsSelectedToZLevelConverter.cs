using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AnnoMapEditor.UI.Converters
{
    [ValueConversion(typeof(bool), typeof(int))]
    class IsSelectedToZLevel : MarkupExtension, IValueConverter
    {
        private static IsSelectedToZLevel? _converter;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected ? 1000 : 0;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null)
                _converter = new IsSelectedToZLevel();
            return (IsSelectedToZLevel)_converter;
        }
    }
}

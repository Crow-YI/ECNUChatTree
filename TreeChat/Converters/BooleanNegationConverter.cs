// BooleanNegationConverter.cs（新建转换器类）
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TreeChat.Converters
{
    public class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }
    }
}
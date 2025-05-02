using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameClubManager.Admin.Converters
{
    public class EnumEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            // Приводим значения к строкам для сравнения
            string valueStr = value.ToString();
            string parameterStr = parameter.ToString();

            // Сравниваем строковые представления
            return valueStr.Equals(parameterStr, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
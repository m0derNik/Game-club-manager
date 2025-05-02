using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameClubManager.Client.Converters
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = System.Convert.ToBoolean(value);
            
            // Инвертируем значение
            boolValue = !boolValue;
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            
            // Инвертируем значение
            return visibility != Visibility.Visible;
        }
    }
} 



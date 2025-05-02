using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameClubManager.Client.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = System.Convert.ToBoolean(value);
            
            // Если параметр указан - инвертируем
            if (parameter != null && parameter.ToString().ToLower() == "invert")
            {
                boolValue = !boolValue;
            }
            
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            
            bool result = visibility == Visibility.Visible;
            
            // Если параметр указан - инвертируем
            if (parameter != null && parameter.ToString().ToLower() == "invert")
            {
                result = !result;
            }
            
            return result;
        }
    }
} 
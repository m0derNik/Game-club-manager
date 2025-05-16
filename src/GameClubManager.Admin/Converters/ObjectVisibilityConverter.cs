using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameClubManager.Admin.Converters
{
    public class ObjectVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = value != null;
            
            // Если есть параметр, то инвертируем результат
            if (parameter is string paramString && 
                (paramString.Equals("Invert", StringComparison.OrdinalIgnoreCase) ||
                paramString.Equals("Inverse", StringComparison.OrdinalIgnoreCase)))
            {
                isVisible = !isVisible;
            }
            
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
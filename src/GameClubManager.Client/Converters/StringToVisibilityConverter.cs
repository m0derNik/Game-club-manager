using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameClubManager.Client.Converters
{
    /// <summary>
    /// Конвертирует строку в Visibility: если строка пустая, то Collapsed, иначе Visible
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrWhiteSpace(stringValue) 
                    ? Visibility.Collapsed 
                    : Visibility.Visible;
            }
            
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Обратное преобразование не требуется
            throw new NotImplementedException();
        }
    }
} 
using System;
using System.Globalization;
using System.Windows.Data;

namespace GameClubManager.Admin.Converters
{
    public class EqualityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
                return true;
                
            if (value == null || parameter == null)
                return false;
                
            // Если оба значения строки, сравниваем их
            if (value is string valueString && parameter is string paramString)
                return valueString.Equals(paramString);
                
            // В противном случае сравниваем как объекты
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
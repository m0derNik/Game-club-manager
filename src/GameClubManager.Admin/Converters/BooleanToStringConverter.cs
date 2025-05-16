using System;
using System.Globalization;
using System.Windows.Data;

namespace GameClubManager.Admin.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public string TrueValue { get; set; } = "Да";
        public string FalseValue { get; set; } = "Нет";
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // Если параметр передан, используем его для возможности переопределения значений
                if (parameter is string paramString)
                {
                    var parts = paramString.Split(';');
                    if (parts.Length == 2)
                    {
                        return boolValue ? parts[0] : parts[1];
                    }
                }
                
                // Используем свойства TrueValue и FalseValue
                return boolValue ? TrueValue : FalseValue;
            }
            
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                if (stringValue.Equals(TrueValue, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (stringValue.Equals(FalseValue, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            
            throw new NotImplementedException();
        }
    }
} 
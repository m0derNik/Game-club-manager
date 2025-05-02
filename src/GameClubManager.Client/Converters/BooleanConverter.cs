using System;
using System.Globalization;
using System.Windows.Data;

namespace GameClubManager.Client.Converters
{
    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = System.Convert.ToBoolean(value);
            
            // Если параметр начинается с восклицательного знака, инвертируем значение
            if (parameter != null && parameter.ToString().StartsWith("!"))
            {
                boolValue = !boolValue;
                
                // Убираем восклицательный знак для дальнейшей обработки
                string param = parameter.ToString().Substring(1);
                
                // Проверяем, есть ли еще параметры
                if (!string.IsNullOrEmpty(param))
                {
                    parameter = param;
                }
                else
                {
                    return boolValue;
                }
            }
            
            // Если есть дополнительный параметр - проверяем, что он равен true
            if (parameter != null)
            {
                bool additionalParam = false;
                bool.TryParse(parameter.ToString(), out additionalParam);
                
                return boolValue && additionalParam;
            }
            
            return boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToBoolean(value);
        }
    }
} 
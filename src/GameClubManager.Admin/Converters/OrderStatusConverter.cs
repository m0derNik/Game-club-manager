using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GameClubManager.Shared.Models;

namespace GameClubManager.Admin.Pages
{
    // Конвертер для преобразования OrderStatus в строку
    public class OrderStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Pending => "Ожидает",
                    OrderStatus.Processing => "В обработке",
                    OrderStatus.Delivered => "Доставлен",
                    OrderStatus.Canceled => "Отменен",
                    _ => "Неизвестно"
                };
            }
            
            return "Неизвестно";
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Конвертер для преобразования OrderStatus в цвет
    public class OrderStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Pending => new SolidColorBrush(Color.FromRgb(33, 150, 243)),    // Синий
                    OrderStatus.Processing => new SolidColorBrush(Color.FromRgb(255, 152, 0)),  // Оранжевый
                    OrderStatus.Delivered => new SolidColorBrush(Color.FromRgb(76, 175, 80)),   // Зеленый
                    OrderStatus.Canceled => new SolidColorBrush(Color.FromRgb(244, 67, 54)),    // Красный
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))                      // Серый
                };
            }
            
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Конвертер для сравнения OrderStatus с заданным значением
    public class EnumEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return System.Windows.Visibility.Collapsed;
                
            bool isEqual = value.Equals(parameter);
            return isEqual ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    // Конвертер для сравнения OrderStatus с несколькими значениями
    public class MultiEnumEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return System.Windows.Visibility.Collapsed;
                
            string paramString = parameter.ToString();
            if (string.IsNullOrEmpty(paramString))
                return System.Windows.Visibility.Collapsed;
                
            string[] values = paramString.Split(',');
            
            foreach (string val in values)
            {
                if (Enum.TryParse<OrderStatus>(val.Trim(), out OrderStatus enumValue))
                {
                    if (value.Equals(enumValue))
                        return System.Windows.Visibility.Visible;
                }
            }
            
            return System.Windows.Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 
using System;
using System.Windows.Media.Imaging;

namespace GameClubManager.Client.Models
{
    public class FoodItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public FoodCategory Category { get; set; }
        
        // Вычисляемое свойство для отображения
        public BitmapImage? ImageSource
        {
            get
            {
                if (string.IsNullOrEmpty(ImageUrl))
                    return null;
                
                try
                {
                    return new BitmapImage(new Uri(ImageUrl));
                }
                catch
                {
                    return null;
                }
            }
        }
    }
    
    public enum FoodCategory
    {
        Food,
        Drink,
        Snack
    }
}
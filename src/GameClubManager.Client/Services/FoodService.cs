using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Client.Models;
using GameClubManager.Shared.Models;

namespace GameClubManager.Client.Services
{
    public class FoodService
    {
        private static FoodService? _instance;
        private readonly TimeService _timeService;
        private readonly ApiService _apiService;
        private readonly AuthManager _authManager;
        private readonly ObservableCollection<CartItem> _cart = new ObservableCollection<CartItem>();
        
        public static FoodService Instance => _instance ??= new FoodService();
        
        public ObservableCollection<CartItem> Cart => _cart;
        
        public decimal CartTotal => _cart.Sum(item => item.TotalPrice);
        
        public List<Client.Models.FoodItem> AvailableFoodItems { get; private set; } = new List<Client.Models.FoodItem>();
        
        private FoodService()
        {
            _timeService = TimeService.Instance;
            _apiService = ApiService.Instance;
            _authManager = AuthManager.Instance;
            LoadFoodItems();
        }
        
        private async void LoadFoodItems()
        {
            try
            {
                // Запрашиваем только доступные продукты для клиента
                var serverFoodItems = await _apiService.GetFoodItemsAsync(true);
                
                if (serverFoodItems?.Count > 0)
                {
                    AvailableFoodItems = serverFoodItems;
                }
                else
                {
                    // Если нет доступных продуктов или произошла ошибка, используем мок-данные
                    System.Windows.MessageBox.Show("Не удалось загрузить список продуктов с сервера. Используются демонстрационные данные.", 
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AvailableFoodItems = GetMockFoodItems();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при загрузке продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Используем мок-данные в случае ошибки
                AvailableFoodItems = GetMockFoodItems();
            }
        }
        
        private Client.Models.FoodCategory GetCategoryForFood(string name)
        {
            if (name.Contains("пицца", StringComparison.OrdinalIgnoreCase) || 
                name.Contains("бургер", StringComparison.OrdinalIgnoreCase))
                return Client.Models.FoodCategory.Food;
                
            if (name.Contains("кола", StringComparison.OrdinalIgnoreCase) || 
                name.Contains("напиток", StringComparison.OrdinalIgnoreCase))
                return Client.Models.FoodCategory.Drink;
                
            return Client.Models.FoodCategory.Snack;
        }
        
        public void AddToCart(Client.Models.FoodItem foodItem, int quantity = 1)
        {
            // Проверяем, есть ли товар уже в корзине
            var existingItem = _cart.FirstOrDefault(item => item.FoodItem.Id == foodItem.Id);
            if (existingItem != null)
            {
                // Увеличиваем количество
                existingItem.Quantity += quantity;
            }
            else
            {
                // Добавляем новый товар
                _cart.Add(new CartItem(foodItem, quantity));
            }
        }
        
        public void RemoveFromCart(int foodItemId)
        {
            var item = _cart.FirstOrDefault(item => item.FoodItem.Id == foodItemId);
            if (item != null)
            {
                _cart.Remove(item);
            }
        }
        
        public void UpdateCartItemQuantity(int foodItemId, int quantity)
        {
            var item = _cart.FirstOrDefault(item => item.FoodItem.Id == foodItemId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }
        
        public void ClearCart()
        {
            _cart.Clear();
        }
        
        public async Task<bool> PurchaseCartItems()
        {
            try
            {
                if (_cart.Count == 0)
                {
                    System.Windows.MessageBox.Show("Корзина пуста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                
                // Проверяем, хватает ли средств на покупку
                var total = CartTotal;
                if (_timeService.Balance < total)
                {
                    System.Windows.MessageBox.Show($"Недостаточно средств на счете. Необходимо: {total:C}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                
                // Создаем запрос на создание заказа
                var orderRequest = new OrderRequest
                {
                    UserId = _authManager.CurrentUser?.Id ?? 0,
                    TotalAmount = total,
                    DeliveryLocation = _authManager.CurrentComputer != null 
                        ? $"Компьютер {_authManager.CurrentComputer.Name}" 
                        : "Местоположение не указано",
                    Items = _cart.Select(item => new Shared.Models.OrderItemRequest
                    {
                        Id = item.FoodItem.Id,
                        Quantity = item.Quantity,
                        Price = item.FoodItem.Price
                    }).ToList()
                };
                
                // Отправляем заказ на сервер
                var orderResponse = await _apiService.CreateOrderAsync(orderRequest);
                if (orderResponse == null)
                {
                    System.Windows.MessageBox.Show("Не удалось создать заказ. Повторите попытку позже.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                
                // Списываем средства
                _timeService.AddBalance(-total);
                
                // Очищаем корзину
                _cart.Clear();
                
                System.Windows.MessageBox.Show($"Заказ #{orderResponse.Id} оформлен на сумму {total:C}. Ожидайте доставки.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Метод для получения списка заказов пользователя
        public async Task<List<OrderResponse>> GetUserOrders()
        {
            if (_authManager.CurrentUser == null)
                return new List<OrderResponse>();
                
            return await _apiService.GetUserOrdersAsync(_authManager.CurrentUser.Id);
        }
        
        // Метод для отмены заказа
        public async Task<bool> CancelOrder(int orderId)
        {
            return await _apiService.CancelOrderAsync(orderId);
        }
        
        // Временные данные для тестирования
        private List<Client.Models.FoodItem> GetMockFoodItems()
        {
            return new List<Client.Models.FoodItem>
            {
                new Client.Models.FoodItem
                {
                    Id = 1,
                    Name = "Пицца Пепперони",
                    Description = "Классическая пицца с колбасой пепперони, сыром и томатным соусом",
                    Price = 400,
                    ImageUrl = "https://via.placeholder.com/150",
                    Category = Client.Models.FoodCategory.Food
                },
                new Client.Models.FoodItem
                {
                    Id = 2,
                    Name = "Кола",
                    Description = "Газированный напиток, 0.5л",
                    Price = 120,
                    ImageUrl = "https://via.placeholder.com/150",
                    Category = Client.Models.FoodCategory.Drink
                },
                new Client.Models.FoodItem
                {
                    Id = 3,
                    Name = "Чипсы Lays",
                    Description = "Картофельные чипсы с солью, 80г",
                    Price = 150,
                    ImageUrl = "https://via.placeholder.com/150",
                    Category = Client.Models.FoodCategory.Snack
                },
                new Client.Models.FoodItem
                {
                    Id = 4,
                    Name = "Энергетический напиток Monster",
                    Description = "Энергетический напиток, 0.5л",
                    Price = 180,
                    ImageUrl = "https://via.placeholder.com/150",
                    Category = Client.Models.FoodCategory.Drink
                },
                new Client.Models.FoodItem
                {
                    Id = 5,
                    Name = "Бургер",
                    Description = "Сочный бургер с говяжьей котлетой, сыром и овощами",
                    Price = 350,
                    ImageUrl = "https://via.placeholder.com/150",
                    Category = Client.Models.FoodCategory.Food
                },
                new Client.Models.FoodItem
                {
                    Id = 6,
                    Name = "Шоколадный батончик Snickers",
                    Description = "Батончик с карамелью, арахисом и нугой, 50г",
                    Price = 90,
                    ImageUrl = "https://via.placeholder.com/150",
                    Category = Client.Models.FoodCategory.Snack
                }
            };
        }
    }
} 



using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Admin.Models;
using AdminFoodItem = GameClubManager.Admin.Models.FoodItem;
using SharedFoodItem = GameClubManager.Shared.Models.FoodItem;
using MessageBox = System.Windows.MessageBox;

namespace GameClubManager.Admin.Services
{
    public class FoodService
    {
        private static FoodService _instance;
        private readonly ApiService _apiService;
        private ObservableCollection<AdminFoodItem> _foodItems;
        private bool _isLoading;

        public static FoodService Instance => _instance ??= new FoodService();

        public ObservableCollection<AdminFoodItem> FoodItems => _foodItems;
        
        public bool IsLoading 
        { 
            get => _isLoading; 
            private set => _isLoading = value; 
        }

        private FoodService()
        {
            _apiService = ApiService.Instance;
            _foodItems = new ObservableCollection<AdminFoodItem>();
        }

        public async Task LoadFoodItemsAsync()
        {
            try
            {
                IsLoading = true;
                var foodItems = await _apiService.GetFoodItemsAsync();
                
                _foodItems.Clear();
                foreach (var item in foodItems)
                {
                    _foodItems.Add(ConvertToAdminModel(item));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке продуктов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task<AdminFoodItem> AddFoodItemAsync(AdminFoodItem foodItem)
        {
            try
            {
                var sharedModel = ConvertToSharedModel(foodItem);
                var createdItem = await _apiService.CreateFoodItemAsync(sharedModel);
                
                if (createdItem != null)
                {
                    var adminModel = ConvertToAdminModel(createdItem);
                    _foodItems.Add(adminModel);
                    return adminModel;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool> UpdateFoodItemAsync(AdminFoodItem foodItem)
        {
            try
            {
                var sharedModel = ConvertToSharedModel(foodItem);
                var success = await _apiService.UpdateFoodItemAsync(foodItem.Id, sharedModel);
                
                if (success)
                {
                    // Обновляем элемент в коллекции
                    var existingItem = _foodItems.FirstOrDefault(t => t.Id == foodItem.Id);
                    if (existingItem != null)
                    {
                        var index = _foodItems.IndexOf(existingItem);
                        _foodItems[index] = foodItem;
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteFoodItemAsync(int foodItemId)
        {
            try
            {
                var success = await _apiService.DeleteFoodItemAsync(foodItemId);
                
                if (success)
                {
                    var itemToRemove = _foodItems.FirstOrDefault(t => t.Id == foodItemId);
                    if (itemToRemove != null)
                    {
                        _foodItems.Remove(itemToRemove);
                    }
                }
                
                return success;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private AdminFoodItem ConvertToAdminModel(SharedFoodItem sharedModel)
        {
            return new AdminFoodItem
            {
                Id = sharedModel.Id,
                Name = sharedModel.Name,
                Description = sharedModel.Description,
                Price = sharedModel.Price,
                ImageUrl = sharedModel.ImageUrl,
                IsAvailable = sharedModel.IsAvailable,
                Category = sharedModel.Category
            };
        }

        private SharedFoodItem ConvertToSharedModel(AdminFoodItem adminModel)
        {
            return new SharedFoodItem
            {
                Id = adminModel.Id,
                Name = adminModel.Name,
                Description = adminModel.Description,
                Price = adminModel.Price,
                ImageUrl = adminModel.ImageUrl,
                IsAvailable = adminModel.IsAvailable,
                Category = adminModel.Category
            };
        }
    }
} 
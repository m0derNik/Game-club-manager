using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;
using GameClubManager.Shared.Models;
using AdminFoodItem = GameClubManager.Admin.Models.FoodItem;
using SharedFoodItem = GameClubManager.Shared.Models.FoodItem;
using MessageBox = System.Windows.MessageBox;

namespace GameClubManager.Admin.ViewModels
{
    public class FoodViewModel : INotifyPropertyChanged
    {
        private readonly FoodService _foodService;
        private AdminFoodItem _selectedFoodItem;
        private bool _isLoading;
        private bool _isEditing;
        private AdminFoodItem _editingFoodItem;
        private string _errorMessage;

        public FoodViewModel()
        {
            _foodService = FoodService.Instance;
            
            // Команды
            LoadCommand = new AsyncRelayCommand(LoadFoodItemsAsync);
            AddCommand = new RelayCommand(AddFoodItem, () => !IsEditing);
            EditCommand = new RelayCommand(EditFoodItem, () => SelectedFoodItem != null && !IsEditing);
            SaveCommand = new AsyncRelayCommand(SaveFoodItemAsync, () => IsEditing);
            CancelCommand = new RelayCommand(CancelEdit, () => IsEditing);
            DeleteCommand = new AsyncRelayCommand(DeleteFoodItemAsync, () => SelectedFoodItem != null && !IsEditing);
            
            // Загружаем продукты при создании
            _ = LoadFoodItemsAsync();
        }

        // Свойства
        public ObservableCollection<AdminFoodItem> FoodItems => _foodService.FoodItems;

        public AdminFoodItem SelectedFoodItem
        {
            get => _selectedFoodItem;
            set
            {
                _selectedFoodItem = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                _isEditing = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public AdminFoodItem EditingFoodItem
        {
            get => _editingFoodItem;
            set
            {
                _editingFoodItem = value;
                OnPropertyChanged();
                
                // Обновляем состояние кнопки сохранения
                (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsAdding => IsEditing && EditingFoodItem?.Id == 0;

        // Команды
        public ICommand LoadCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        // Массив всех значений перечисления FoodCategory для использования в ComboBox
        public Array FoodCategories => Enum.GetValues(typeof(FoodCategory));

        // Методы
        private async Task LoadFoodItemsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                await _foodService.LoadFoodItemsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка загрузки продуктов: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddFoodItem()
        {
            IsEditing = true;
            EditingFoodItem = new AdminFoodItem
            {
                Name = "Новый продукт",
                Price = 0,
                Description = "",
                ImageUrl = "",
                IsAvailable = true,
                Category = FoodCategory.Food
            };
            
            // Подписываемся на изменения свойств
            EditingFoodItem.PropertyChanged += EditingFoodItem_PropertyChanged;
        }

        private void EditFoodItem()
        {
            if (SelectedFoodItem == null) return;
            
            IsEditing = true;
            EditingFoodItem = new AdminFoodItem
            {
                Id = SelectedFoodItem.Id,
                Name = SelectedFoodItem.Name,
                Price = SelectedFoodItem.Price,
                Description = SelectedFoodItem.Description,
                ImageUrl = SelectedFoodItem.ImageUrl,
                IsAvailable = SelectedFoodItem.IsAvailable,
                Category = SelectedFoodItem.Category
            };
            
            // Подписываемся на изменения свойств
            EditingFoodItem.PropertyChanged += EditingFoodItem_PropertyChanged;
        }
        
        private void EditingFoodItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Обновляем состояние кнопки сохранения при изменении свойств редактируемого продукта
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task SaveFoodItemAsync()
        {
            if (EditingFoodItem == null) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                // Проверяем обязательные поля
                if (string.IsNullOrWhiteSpace(EditingFoodItem.Name))
                {
                    ErrorMessage = "Название продукта не может быть пустым";
                    return;
                }
                
                if (EditingFoodItem.Price < 0)
                {
                    ErrorMessage = "Цена не может быть отрицательной";
                    return;
                }

                bool success;
                
                if (EditingFoodItem.Id == 0)
                {
                    // Добавление нового продукта
                    await _foodService.AddFoodItemAsync(EditingFoodItem);
                    success = true;
                }
                else
                {
                    // Обновление существующего продукта
                    success = await _foodService.UpdateFoodItemAsync(EditingFoodItem);
                }
                
                if (success)
                {
                    IsEditing = false;
                    EditingFoodItem = null;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения продукта: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            if (EditingFoodItem != null)
            {
                // Отписываемся от событий
                EditingFoodItem.PropertyChanged -= EditingFoodItem_PropertyChanged;
            }
            
            IsEditing = false;
            EditingFoodItem = null;
            ErrorMessage = string.Empty;
        }

        private async Task DeleteFoodItemAsync()
        {
            if (SelectedFoodItem == null) return;
            
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить продукт '{SelectedFoodItem.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
                
            if (result != MessageBoxResult.Yes) return;
            
            IsLoading = true;
            ErrorMessage = string.Empty;
            
            try
            {
                await _foodService.DeleteFoodItemAsync(SelectedFoodItem.Id);
                SelectedFoodItem = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления продукта: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
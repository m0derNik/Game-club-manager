using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;
using GameClubManager.Admin.Services;
using MessageBox = System.Windows.MessageBox;

namespace GameClubManager.Admin.ViewModels
{
    public class TariffsViewModel : INotifyPropertyChanged
    {
        private readonly TariffService _tariffService;
        private ObservableCollection<Tariff> _tariffs;
        private Tariff _selectedTariff;
        private Tariff _editTariff;
        private bool _isEditing;
        private bool _isAdding;
        private bool _isSaving;

        public TariffsViewModel()
        {
            _tariffService = TariffService.Instance;
            LoadTariffs();
            
            // Инициализация команд
            AddTariffCommand = new RelayCommand(ExecuteAddTariff);
            EditTariffCommand = new RelayCommand<Tariff>(ExecuteEditTariff);
            DeleteTariffCommand = new RelayCommand<Tariff>(ExecuteDeleteTariff);
            SaveTariffCommand = new RelayCommand(ExecuteSaveTariff, CanExecuteSaveTariff);
            CancelEditCommand = new RelayCommand(ExecuteCancelEdit);
        }

        public ObservableCollection<Tariff> Tariffs
        {
            get => _tariffs;
            set
            {
                _tariffs = value;
                OnPropertyChanged();
            }
        }

        public Tariff SelectedTariff
        {
            get => _selectedTariff;
            set
            {
                _selectedTariff = value;
                OnPropertyChanged();
            }
        }

        public Tariff EditTariff
        {
            get => _editTariff;
            set
            {
                _editTariff = value;
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
                (SaveTariffCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public bool IsAdding
        {
            get => _isAdding;
            set
            {
                _isAdding = value;
                OnPropertyChanged();
            }
        }

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                _isSaving = value;
                OnPropertyChanged();
                (SaveTariffCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddTariffCommand { get; }
        public ICommand EditTariffCommand { get; }
        public ICommand DeleteTariffCommand { get; }
        public ICommand SaveTariffCommand { get; }
        public ICommand CancelEditCommand { get; }

        private async void LoadTariffs()
        {
            try
            {
                IsSaving = true;
                await _tariffService.RefreshTariffsAsync();
                Tariffs = new ObservableCollection<Tariff>(_tariffService.Tariffs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке тарифов: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private void ExecuteAddTariff()
        {
            EditTariff = new Tariff
            {
                Id = 0,
                Name = "Новый тариф",
                Description = "Описание тарифа",
                Price = 100,
                Duration = TimeSpan.FromHours(1),
                IsPopular = false
            };
            
            IsEditing = true;
            IsAdding = true;
        }

        private void ExecuteEditTariff(Tariff tariff)
        {
            if (tariff == null) return;
            
            // Создаем копию тарифа для редактирования
            EditTariff = new Tariff
            {
                Id = tariff.Id,
                Name = tariff.Name,
                Description = tariff.Description,
                Price = tariff.Price,
                Duration = tariff.Duration,
                IsPopular = tariff.IsPopular
            };
            
            IsEditing = true;
            IsAdding = false;
        }

        private async void ExecuteSaveTariff()
        {
            if (EditTariff == null) return;
            
            try
            {
                IsSaving = true;
                
                if (IsAdding)
                {
                    _tariffService.AddTariff(EditTariff);
                }
                else
                {
                    _tariffService.UpdateTariff(EditTariff);
                }
                
                // Сохраняем изменения через API
                var success = await _tariffService.SaveTariffsToFile();
                
                if (success)
                {
                    // Обновляем список тарифов
                    LoadTariffs();
                    
                    // Закрываем редактирование
                    IsEditing = false;
                    IsAdding = false;
                    
                    MessageBox.Show(
                        IsAdding ? "Тариф успешно добавлен" : "Тариф успешно обновлен",
                        "Сохранение тарифа",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении тарифа: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private bool CanExecuteSaveTariff()
        {
            return IsEditing && !IsSaving && EditTariff != null && 
                   !string.IsNullOrWhiteSpace(EditTariff.Name) && 
                   EditTariff.Price > 0 && 
                   EditTariff.Duration.TotalMinutes > 0;
        }

        private void ExecuteCancelEdit()
        {
            IsEditing = false;
            IsAdding = false;
            EditTariff = null;
        }

        private async void ExecuteDeleteTariff(Tariff tariff)
        {
            if (tariff == null) return;
            
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить тариф \"{tariff.Name}\"?",
                "Удаление тарифа",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsSaving = true;
                    var success = await _tariffService.DeleteTariff(tariff.Id);
                    
                    if (success)
                    {
                        LoadTariffs();
                        
                        MessageBox.Show(
                            "Тариф успешно удален",
                            "Удаление тарифа",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при удалении тарифа: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    IsSaving = false;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
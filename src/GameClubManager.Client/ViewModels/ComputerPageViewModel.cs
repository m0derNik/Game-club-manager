using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;

namespace GameClubManager.Client.ViewModels
{
    public class ComputerPageViewModel : INotifyPropertyChanged
    {
        private readonly AuthManager _authManager;
        private readonly ComputerBindingService _computerBindingService;
        private bool _isComputerSelectorVisible;
        
        // Текущий компьютер
        public ComputerDto CurrentComputer => _authManager.CurrentComputer;
        
        // Свойство, которое определяет, есть ли выбранный компьютер
        public bool HasCurrentComputer => CurrentComputer != null;
        
        // Свойство для определения видимости селектора компьютеров
        public bool IsComputerSelectorVisible
        {
            get => _isComputerSelectorVisible;
            set
            {
                _isComputerSelectorVisible = value;
                OnPropertyChanged();
                
                // Обновляем зависимые свойства
                OnPropertyChanged(nameof(ShowNoComputerInfo));
                OnPropertyChanged(nameof(ShowComputerDetails));
            }
        }
        
        // Свойство для определения видимости сообщения "компьютер не выбран"
        public bool ShowNoComputerInfo => !HasCurrentComputer && !IsComputerSelectorVisible;
        
        // Свойство для определения видимости детальной информации о компьютере
        public bool ShowComputerDetails => HasCurrentComputer && !IsComputerSelectorVisible;
        
        // Команды
        public ICommand SelectComputerCommand { get; }
        public ICommand UnbindComputerCommand { get; }
        
        public ComputerPageViewModel()
        {
            _authManager = AuthManager.Instance;
            _computerBindingService = ComputerBindingService.Instance;
            
            // Подписываемся на изменения в AuthManager
            _authManager.PropertyChanged += OnAuthManagerPropertyChanged;
            
            // Инициализируем команды
            SelectComputerCommand = new RelayCommand(_ => ShowComputerSelector());
            UnbindComputerCommand = new RelayCommand(_ => UnbindCurrentComputer());
        }
        
        // Метод для отображения селектора компьютеров
        private void ShowComputerSelector()
        {
            IsComputerSelectorVisible = true;
        }
        
        // Метод для привязки компьютера
        public async Task<bool> BindComputerAsync(ComputerDto computer)
        {
            if (computer == null)
                return false;
                
            var result = await _computerBindingService.BindUserToComputer(computer.Id);
            
            // После привязки скрываем селектор, независимо от результата
            IsComputerSelectorVisible = false;
            
            // Обновляем UI
            UpdateUI();
            
            return result;
        }
        
        // Метод для отвязки текущего компьютера
        private async void UnbindCurrentComputer()
        {
            await _computerBindingService.UnbindUserFromComputer();
            
            // Обновляем UI
            UpdateUI();
        }
        
        // Метод для скрытия селектора компьютеров (отмена выбора)
        public void CancelComputerSelection()
        {
            IsComputerSelectorVisible = false;
        }
        
        // Обработчик изменений в AuthManager
        private void OnAuthManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AuthManager.CurrentComputer))
            {
                UpdateUI();
            }
        }
        
        // Обновляет все зависимые свойства в UI
        private void UpdateUI()
        {
            OnPropertyChanged(nameof(CurrentComputer));
            OnPropertyChanged(nameof(HasCurrentComputer));
            OnPropertyChanged(nameof(ShowNoComputerInfo));
            OnPropertyChanged(nameof(ShowComputerDetails));
        }
        
        // Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
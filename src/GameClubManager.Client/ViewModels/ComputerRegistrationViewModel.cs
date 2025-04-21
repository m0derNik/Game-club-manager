using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GameClubManager.Client.Commands;
using GameClubManager.Client.Models;
using GameClubManager.Client.Services;
using System.Windows;

namespace GameClubManager.Client.ViewModels
{
    public class ComputerRegistrationViewModel : INotifyPropertyChanged
    {
        private readonly ComputerRegistrationService _computerRegistrationService;
        private readonly ApiService _apiService;
        private readonly AuthManager _authManager;
        
        private string _name = string.Empty;
        private string _ipAddress = string.Empty;
        private string _macAddress = string.Empty;
        private string _specifications = string.Empty;
        private bool _isRegistering;
        
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
                RegisterComputerCommand.RaiseCanExecuteChanged();
            }
        }
        
        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged();
                RegisterComputerCommand.RaiseCanExecuteChanged();
            }
        }
        
        public string MacAddress
        {
            get => _macAddress;
            set
            {
                _macAddress = value;
                OnPropertyChanged();
                RegisterComputerCommand.RaiseCanExecuteChanged();
            }
        }
        
        public string Specifications
        {
            get => _specifications;
            set
            {
                _specifications = value;
                OnPropertyChanged();
                RegisterComputerCommand.RaiseCanExecuteChanged();
            }
        }
        
        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                _isRegistering = value;
                OnPropertyChanged();
                RegisterComputerCommand.RaiseCanExecuteChanged();
            }
        }
        
        public AsyncRelayCommand RegisterComputerCommand { get; }
        
        public ComputerRegistrationViewModel()
        {
            _apiService = ApiService.Instance;
            _authManager = AuthManager.Instance;
            _computerRegistrationService = new ComputerRegistrationService(_apiService, _authManager);
            
            RegisterComputerCommand = new AsyncRelayCommand(
                RegisterComputerAsync,
                CanRegisterComputer);
        }
        
        private bool CanRegisterComputer()
        {
            return !IsRegistering && 
                   !string.IsNullOrWhiteSpace(Name) && 
                   !string.IsNullOrWhiteSpace(IpAddress) && 
                   !string.IsNullOrWhiteSpace(MacAddress) && 
                   !string.IsNullOrWhiteSpace(Specifications);
        }
        
        private async Task RegisterComputerAsync()
        {
            try
            {
                IsRegistering = true;
                
                var computer = await _computerRegistrationService.RegisterComputerAsync(
                    Name,
                    IpAddress,
                    MacAddress,
                    Specifications);
                
                if (computer != null)
                {
                    MessageBox.Show($"Компьютер \"{computer.Name}\" успешно зарегистрирован!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsRegistering = false;
            }
        }
        
        private void ClearForm()
        {
            Name = string.Empty;
            IpAddress = string.Empty;
            MacAddress = string.Empty;
            Specifications = string.Empty;
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
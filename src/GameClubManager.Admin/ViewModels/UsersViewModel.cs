using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameClubManager.Admin.Commands;
using GameClubManager.Admin.Models;

namespace GameClubManager.Admin.ViewModels
{
    public class UsersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<User> _users;
        private string _searchText;
        private User _selectedUser;

        public UsersViewModel()
        {
            // Временные данные для демонстрации UI
            Users = new ObservableCollection<User>
            {
                new User { Id = 1, Name = "Иван Иванов", Username = "ivan", Balance = 500, Status = "Активен", RegistrationDate = DateTime.Now.AddDays(-30) },
                new User { Id = 2, Name = "Петр Петров", Username = "petr", Balance = 1000, Status = "Активен", RegistrationDate = DateTime.Now.AddDays(-15) },
                new User { Id = 3, Name = "Сергей Сергеев", Username = "sergey", Balance = 200, Status = "Заблокирован", RegistrationDate = DateTime.Now.AddDays(-7) }
            };

            AddUserCommand = new RelayCommand(ExecuteAddUser);
            EditUserCommand = new RelayCommand<User>(ExecuteEditUser);
            DeleteUserCommand = new RelayCommand<User>(ExecuteDeleteUser);
        }

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                // Здесь будет логика фильтрации
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        private void ExecuteAddUser()
        {
            // Здесь будет логика добавления пользователя
        }

        private void ExecuteEditUser(User user)
        {
            // Здесь будет логика редактирования пользователя
        }

        private void ExecuteDeleteUser(User user)
        {
            // Здесь будет логика удаления пользователя
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
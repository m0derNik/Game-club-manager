using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GameClubManager.Client.Models;
using System.Windows;
using System.Net.Http.Json;

namespace GameClubManager.Client.Services
{
    public class ComputerService
    {
        private static ComputerService? _instance;
        private readonly ApiService _apiService;
        
        public static ComputerService Instance => _instance ??= new ComputerService();
        
        private ComputerService()
        {
            _apiService = ApiService.Instance;
        }
        
        public async Task<List<Models.ComputerDto>> GetAllComputersAsync()
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("http://localhost:7001/api/computers");
                response.EnsureSuccessStatusCode();
                var serverResponse = await response.Content.ReadFromJsonAsync<dynamic>();
                
                var computers = new List<Models.ComputerDto>();
                foreach (var item in serverResponse)
                {
                    computers.Add(new Models.ComputerDto
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Specifications = item.Specifications,
                        PricePerHour = item.PricePerHour,
                        Status = Models.ComputerStatus.Available,
                        CurrentUserId = item.CurrentUserId
                    });
                }
                
                return computers;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении списка компьютеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Models.ComputerDto>();
            }
        }
        
        public async Task<Models.ComputerDto?> GetComputerByIdAsync(int computerId)
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync($"http://localhost:7001/api/computers/{computerId}");
                response.EnsureSuccessStatusCode();
                var serverResponse = await response.Content.ReadFromJsonAsync<dynamic>();
                
                return new Models.ComputerDto
                {
                    Id = serverResponse.Id,
                    Name = serverResponse.Name,
                    Specifications = serverResponse.Specifications,
                    PricePerHour = serverResponse.PricePerHour,
                    Status = Models.ComputerStatus.Available,
                    CurrentUserId = serverResponse.CurrentUserId
                };
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при получении данных о компьютере: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
        
        public async Task<bool> BookComputerAsync(int computerId, int userId, int timeInMinutes)
        {
            try
            {
                var httpClient = new HttpClient();
                var request = new
                {
                    ComputerId = computerId,
                    UserId = userId,
                    TimeInMinutes = timeInMinutes
                };
                
                var response = await httpClient.PostAsJsonAsync("http://localhost:7001/api/computers/book", request);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при бронировании компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        public async Task<bool> ReleaseComputerAsync(int computerId)
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync($"http://localhost:7001/api/computers/{computerId}/release", null);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при освобождении компьютера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        public async Task<bool> UpdateComputerAsync(Models.ComputerDto computer)
        {
            try
            {
                var httpClient = new HttpClient();
                
                // Преобразуем наши DTO в формат, понятный серверу
                var requestData = new
                {
                    Id = computer.Id,
                    Name = computer.Name,
                    Specifications = computer.Specifications,
                    PricePerHour = computer.PricePerHour,
                    Status = computer.Status.ToString()
                };
                
                var response = await httpClient.PutAsJsonAsync($"http://localhost:7001/api/computers/{computer.Id}", requestData);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при обновлении данных о компьютере: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
} 



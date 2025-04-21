using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using GameClubManager.Admin.Models;

namespace GameClubManager.Admin.Services
{
    public class ComputerService
    {
        private static ComputerService _instance;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:7001/api";

        public static ComputerService Instance => _instance ??= new ComputerService();

        private ComputerService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        // Получение списка всех компьютеров с сервера
        public async Task<List<Computer>> GetAllComputersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/computers");
                response.EnsureSuccessStatusCode();

                var computerDtos = await response.Content.ReadFromJsonAsync<List<ComputerDto>>();
                if (computerDtos == null) return new List<Computer>();

                var result = new List<Computer>();
                foreach (var dto in computerDtos)
                {
                    result.Add(new Computer
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        Status = GetStatusText(dto.Status, dto.LastActivity),
                        CurrentUser = dto.CurrentUserName ?? "-",
                        TimeRemaining = GetTimeRemainingText(dto)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения списка компьютеров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Computer>();
            }
        }

        // Отправка команд на перезапуск/выключение компьютеров будет добавлена позже
        public async Task<bool> RestartComputerAsync(int computerId)
        {
            MessageBox.Show("Функция перезапуска компьютера будет доступна в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        public async Task<bool> ShutdownComputerAsync(int computerId)
        {
            MessageBox.Show("Функция выключения компьютера будет доступна в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        // Вспомогательные методы для форматирования информации
        private string GetStatusText(string status, DateTime lastActivity)
        {
            // Проверяем активность компьютера
            var timeSinceLastActivity = DateTime.UtcNow - lastActivity;
            
            // Если последняя активность была более 5 минут назад, считаем компьютер офлайн
            if (timeSinceLastActivity.TotalMinutes > 5)
            {
                return "Офлайн";
            }

            switch (status)
            {
                case "InUse": return "Занят";
                case "Available": return "Свободен";
                case "Maintenance": return "Обслуживание";
                case "OutOfOrder": return "Не работает";
                default: return "Неизвестно";
            }
        }

        private string GetTimeRemainingText(ComputerDto dto)
        {
            // Если компьютер не используется, возвращаем "-"
            if (dto.Status != "InUse" || dto.CurrentUserId == null)
            {
                return "-";
            }

            // В реальном приложении здесь должна быть логика расчета оставшегося времени
            // на основе информации о пользователе, его баланса и т.д.
            // Пока возвращаем заглушку
            return "1:30"; // временная заглушка
        }
    }

    // DTO для обмена данными с сервером
    public class ComputerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int? CurrentUserId { get; set; }
        public string CurrentUserName { get; set; }
        public DateTime LastActivity { get; set; }
        public string Specifications { get; set; }
        public decimal PricePerHour { get; set; }
    }
} 
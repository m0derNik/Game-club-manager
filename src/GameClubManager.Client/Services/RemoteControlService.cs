using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Json;
using GameClubManager.Shared.Models;
using LZ4;

namespace GameClubManager.Client.Services
{
    public class RemoteControlService
    {
        private static RemoteControlService _instance;
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private System.Threading.Timer _screenshotTimer;
        private bool _isSessionActive = false;
        private int _computerId;

        private const string BaseUrl = "http://localhost:7001/api";
        private const int ScreenshotInterval = 200; // 5 fps

        // Статический экземпляр для паттерна Singleton
        public static RemoteControlService Instance => _instance ??= new RemoteControlService();

        private RemoteControlService()
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

        #region Windows API для эмуляции ввода
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const int MOUSEEVENTF_MIDDLEUP = 0x40;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;
        #endregion

        // Регистрация компьютера для удаленного управления
        public async Task RegisterForRemoteControlAsync()
        {
            try
            {
                // Получаем идентификатор компьютера из файла конфигурации или создаем новый
                _computerId = GetLocalComputerId();
                
                if (_computerId == 0)
                {
                    // Зарегистрировать компьютер, если его еще нет в системе
                    var registrationRequest = new Models.ComputerRegistrationRequest
                    {
                        Name = Environment.MachineName,
                        Specifications = GetSystemSpecifications(),
                        PricePerHour = 100 // Значение по умолчанию
                    };
                    
                    var result = await ApiService.Instance.RegisterComputerAsync(registrationRequest);
                    if (result != null)
                    {
                        _computerId = result.Id;
                        SaveLocalComputerId(_computerId);
                    }
                }
                
                // Запускаем прослушивание команд удаленного управления
                await StartListeningForRemoteCommands();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка регистрации для удаленного управления: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Получение идентификатора компьютера из файла конфигурации
        private int GetLocalComputerId()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "computer_id.txt");
                if (File.Exists(configPath))
                {
                    string content = File.ReadAllText(configPath);
                    if (int.TryParse(content, out int id))
                    {
                        return id;
                    }
                }
            }
            catch { }
            
            return 0;
        }

        // Сохранение идентификатора компьютера в файл конфигурации
        private void SaveLocalComputerId(int id)
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "computer_id.txt");
                File.WriteAllText(configPath, id.ToString());
            }
            catch { }
        }

        // Получение спецификаций системы
        private string GetSystemSpecifications()
        {
            try
            {
                // Простой способ получения базовой информации о системе
                var osVersion = Environment.OSVersion;
                var processorCount = Environment.ProcessorCount;
                var memoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
                
                return $"OS: {osVersion}, CPU Cores: {processorCount}, RAM: {memoryMB} MB";
            }
            catch
            {
                return "Не удалось получить спецификации";
            }
        }

        // Получение скриншота экрана
        private RemoteDesktopData CaptureScreen()
        {
            try
            {
                var bounds = Screen.PrimaryScreen.Bounds;
                using (var bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(
                            bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, ImageFormat.Jpeg);
                        var imageBytes = memoryStream.ToArray();
                        
                        // Сжимаем изображение с помощью LZ4
                        var compressedBytes = LZ4Codec.Encode(imageBytes, 0, imageBytes.Length);
                        
                        return new RemoteDesktopData
                        {
                            ComputerId = _computerId,
                            ScreenData = compressedBytes,
                            IsCompressed = true,
                            Width = bounds.Width,
                            Height = bounds.Height,
                            Timestamp = DateTime.UtcNow
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при создании скриншота: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // Отправка скриншота на сервер
        private async Task SendScreenshotAsync()
        {
            try
            {
                // Создаем скриншот экрана
                using var bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                
                // Конвертируем в байты и сжимаем
                using var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Jpeg);
                byte[] imageBytes = stream.ToArray();
                
                // Сжимаем с помощью LZ4
                byte[] compressedBytes = LZ4Codec.Encode(imageBytes, 0, imageBytes.Length);
                
                // Создаем объект для отправки
                var desktopData = new RemoteDesktopData
                {
                    ComputerId = _computerId,
                    ScreenData = compressedBytes,
                    IsCompressed = true,
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    Timestamp = DateTime.UtcNow
                };
                
                // Отправляем на сервер
                await _httpClient.PostAsJsonAsync($"{BaseUrl}/remotecontrol/public-screenshot", desktopData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке скриншота: {ex.Message}");
            }
        }

        // Начало прослушивания команд удаленного управления
        private async Task StartListeningForRemoteCommands()
        {
            try
            {
                // Отправляем запрос на начало сессии
                await _httpClient.PostAsync($"{BaseUrl}/remotecontrol/{_computerId}/public-start", null);
                _isSessionActive = true;
                
                _cancellationTokenSource = new CancellationTokenSource();
                
                // Запускаем периодическую отправку скриншотов
                _screenshotTimer = new System.Threading.Timer(async _ => await SendScreenshotAsync(), 
                    null, 0, ScreenshotInterval);
                
                // В отдельном потоке слушаем команды от сервера
                await Task.Run(async () =>
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        try
                        {
                            // Запрашиваем новые команды с сервера
                            var response = await _httpClient.GetAsync(
                                $"{BaseUrl}/remotecontrol/{_computerId}/public-commands");
                            
                            if (response.IsSuccessStatusCode)
                            {
                                var commands = await response.Content.ReadFromJsonAsync<List<RemoteInput>>();
                                if (commands != null && commands.Count > 0)
                                {
                                    // Обрабатываем полученные команды
                                    foreach (var command in commands)
                                    {
                                        ProcessRemoteCommand(command);
                                    }
                                }
                            }
                            
                            // Небольшая задержка перед следующим запросом
                            await Task.Delay(50, _cancellationTokenSource.Token);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка получения команд: {ex.Message}");
                            await Task.Delay(1000, _cancellationTokenSource.Token);
                        }
                    }
                }, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка запуска удаленного управления: {ex.Message}");
            }
        }

        // Обработка команды удаленного управления
        private void ProcessRemoteCommand(RemoteInput command)
        {
            switch (command.Type)
            {
                case InputType.MouseMove:
                    SetCursorPos(command.X, command.Y);
                    break;
                    
                case InputType.MouseClick:
                    SetCursorPos(command.X, command.Y);
                    if (command.Button == 0) // Левая кнопка
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, command.X, command.Y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP, command.X, command.Y, 0, 0);
                    }
                    else if (command.Button == 1) // Правая кнопка
                    {
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, command.X, command.Y, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP, command.X, command.Y, 0, 0);
                    }
                    else if (command.Button == 2) // Средняя кнопка
                    {
                        mouse_event(MOUSEEVENTF_MIDDLEDOWN, command.X, command.Y, 0, 0);
                        mouse_event(MOUSEEVENTF_MIDDLEUP, command.X, command.Y, 0, 0);
                    }
                    break;
                    
                case InputType.MouseDown:
                    if (command.Button == 0) // Левая кнопка
                        mouse_event(MOUSEEVENTF_LEFTDOWN, command.X, command.Y, 0, 0);
                    else if (command.Button == 1) // Правая кнопка
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, command.X, command.Y, 0, 0);
                    else if (command.Button == 2) // Средняя кнопка
                        mouse_event(MOUSEEVENTF_MIDDLEDOWN, command.X, command.Y, 0, 0);
                    break;
                    
                case InputType.MouseUp:
                    if (command.Button == 0) // Левая кнопка
                        mouse_event(MOUSEEVENTF_LEFTUP, command.X, command.Y, 0, 0);
                    else if (command.Button == 1) // Правая кнопка
                        mouse_event(MOUSEEVENTF_RIGHTUP, command.X, command.Y, 0, 0);
                    else if (command.Button == 2) // Средняя кнопка
                        mouse_event(MOUSEEVENTF_MIDDLEUP, command.X, command.Y, 0, 0);
                    break;
                    
                case InputType.MouseWheel:
                    mouse_event(MOUSEEVENTF_WHEEL, command.X, command.Y, command.KeyCode, 0);
                    break;
                    
                case InputType.KeyDown:
                    keybd_event((byte)command.KeyCode, 0, KEYEVENTF_KEYDOWN, 0);
                    break;
                    
                case InputType.KeyUp:
                    keybd_event((byte)command.KeyCode, 0, KEYEVENTF_KEYUP, 0);
                    break;
                    
                case InputType.KeyPress:
                    keybd_event((byte)command.KeyCode, 0, KEYEVENTF_KEYDOWN, 0);
                    keybd_event((byte)command.KeyCode, 0, KEYEVENTF_KEYUP, 0);
                    break;
            }
        }

        // Остановка удаленного управления
        public async Task StopRemoteControl()
        {
            try
            {
                // Отправляем запрос на остановку сессии
                if (_isSessionActive)
                {
                    await _httpClient.PostAsync($"{BaseUrl}/remotecontrol/{_computerId}/public-stop", null);
                    _isSessionActive = false;
                }
                
                _screenshotTimer?.Dispose();
                _cancellationTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при остановке удаленного управления: {ex.Message}");
            }
        }

        // Запрос на сервер о статусе сессии
        public async Task CheckRemoteSessionStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/remotecontrol/{_computerId}/status");
                if (response.IsSuccessStatusCode)
                {
                    var status = await response.Content.ReadFromJsonAsync<bool>();
                    
                    // Если статус сессии изменился
                    if (status != _isSessionActive)
                    {
                        _isSessionActive = status;
                        
                        if (_isSessionActive)
                        {
                            // Начинаем отправку скриншотов
                            if (_screenshotTimer == null)
                            {
                                await StartListeningForRemoteCommands();
                            }
                        }
                        else
                        {
                            // Останавливаем отправку скриншотов
                            await StopRemoteControl();
                        }
                    }
                }
            }
            catch { }
        }
    }
} 



using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using GameClubManager.Shared.Models;
using LZ4;

namespace GameClubManager.Admin.Services
{
    // Класс для передачи данных о скриншоте через событие
    public class ScreenshotEventArgs : EventArgs
    {
        public BitmapSource Screenshot { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class RemoteDesktopService
    {
        private static RemoteDesktopService _instance;
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private System.Threading.Timer _refreshTimer;
        private int _computerId;
        private bool _isConnected;
        
        private const string BaseUrl = "http://localhost:7001/api";
        private const int RefreshInterval = 100; // 10 кадров в секунду
        
        // Событие для оповещения об обновлении скриншота
        public event EventHandler<ScreenshotEventArgs> ScreenshotUpdated;
        
        // Событие для оповещения об отключении
        public event EventHandler OnDisconnected;
        
        // Статический экземпляр для паттерна Singleton
        public static RemoteDesktopService Instance => _instance ??= new RemoteDesktopService();
        
        // Свойство для статуса подключения
        public bool IsConnected => _isConnected;
        
        private RemoteDesktopService()
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
        
        // Подключение к удаленному компьютеру
        public async Task<bool> ConnectAsync(int computerId)
        {
            try
            {
                // Если уже подключены к другому компьютеру, отключаемся
                if (_isConnected && _computerId != computerId)
                {
                    await DisconnectAsync();
                }
                
                _computerId = computerId;
                
                // Начинаем сессию удаленного управления
                var response = await _httpClient.PostAsync($"{BaseUrl}/remotecontrol/{computerId}/public-start", null);
                if (!response.IsSuccessStatusCode)
                {
                    System.Windows.MessageBox.Show($"Ошибка при подключении к компьютеру: {response.StatusCode}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                
                // Запускаем обновление скриншотов
                StartScreenshotUpdates();
                
                _isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при подключении к компьютеру: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Отключение от удаленного компьютера
        public async Task DisconnectAsync()
        {
            if (!_isConnected)
                return;
            
            try
            {
                // Останавливаем обновление скриншотов
                StopScreenshotUpdates();
                
                // Отправляем запрос на остановку сессии
                await _httpClient.PostAsync($"{BaseUrl}/remotecontrol/{_computerId}/public-stop", null);
                
                _isConnected = false;
                
                // Оповещаем об отключении
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении от компьютера: {ex.Message}");
            }
        }
        
        // Подключение к удаленному компьютеру по имени
        public async Task<bool> ConnectToComputerAsync(string computerName)
        {
            try
            {
                // Получаем информацию о компьютере по имени
                var computerService = ComputerService.Instance;
                var computers = await computerService.GetAllComputersAsync();
                var computer = computers.Find(c => c.Name == computerName);
                
                if (computer == null)
                {
                    System.Windows.MessageBox.Show($"Компьютер с именем '{computerName}' не найден", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                
                // Подключаемся к найденному компьютеру
                return await ConnectAsync(computer.Id);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при подключении к компьютеру: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Запуск обновления скриншотов
        private void StartScreenshotUpdates()
        {
            _refreshTimer?.Dispose();
            
            // Создаем таймер для периодического запроса скриншотов
            _refreshTimer = new System.Threading.Timer(async _ =>
            {
                try
                {
                    var response = await _httpClient.GetAsync($"{BaseUrl}/remotecontrol/{_computerId}/public-screenshot");
                    if (response.IsSuccessStatusCode)
                    {
                        var screenshotData = await response.Content.ReadFromJsonAsync<RemoteDesktopData>();
                        if (screenshotData != null)
                        {
                            var bitmap = DecodeScreenshot(screenshotData);
                            
                            // Передаем скриншот через событие
                            ScreenshotUpdated?.Invoke(this, new ScreenshotEventArgs
                            {
                                Screenshot = bitmap,
                                Width = screenshotData.Width,
                                Height = screenshotData.Height,
                                Timestamp = screenshotData.Timestamp
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Игнорируем ошибки сети для бесперебойной работы
                    Console.WriteLine($"Ошибка при получении скриншота: {ex.Message}");
                }
            }, null, 0, RefreshInterval);
        }
        
        // Остановка таймера обновления скриншотов
        private void StopScreenshotUpdates()
        {
            _refreshTimer?.Dispose();
            _cancellationTokenSource?.Cancel();
        }
        
        // Декодирование данных скриншота в изображение
        private BitmapImage DecodeScreenshot(RemoteDesktopData screenshotData)
        {
            try
            {
                byte[] imageBytes;
                
                // Если данные сжаты, распаковываем их
                if (screenshotData.IsCompressed)
                {
                    // Оценка размера декодированных данных (обычно для JPEG будет меньше 5 МБ)
                    int estimatedSize = 5 * 1024 * 1024; // 5 МБ
                    imageBytes = new byte[estimatedSize];
                    
                    // Декодирование данных используя правильную перегрузку метода
                    int decodedSize = LZ4Codec.Decode(
                        screenshotData.ScreenData, 0, screenshotData.ScreenData.Length,
                        imageBytes, 0, estimatedSize);
                    
                    // Если декодированный размер меньше выделенного буфера, создаем новый массив нужного размера
                    if (decodedSize < estimatedSize)
                    {
                        byte[] resizedArray = new byte[decodedSize];
                        Array.Copy(imageBytes, resizedArray, decodedSize);
                        imageBytes = resizedArray;
                    }
                }
                else
                {
                    imageBytes = screenshotData.ScreenData;
                }
                
                // Создаем изображение из байтов
                var bitmap = new BitmapImage();
                using (var stream = new MemoryStream(imageBytes))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Важно заморозить изображение для потокобезопасности
                }
                
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка декодирования скриншота: {ex.Message}");
                return null;
            }
        }
        
        // Отправка команды мыши
        public async Task SendMouseMoveAsync(int x, int y)
        {
            if (!_isConnected)
                return;
            
            var command = new RemoteInput
            {
                ComputerId = _computerId,
                Type = InputType.MouseMove,
                X = x,
                Y = y
            };
            
            await SendCommandAsync(command);
        }
        
        // Отправка команды клика мыши
        public async Task SendMouseClickAsync(int x, int y, int button)
        {
            if (!_isConnected)
                return;
            
            var command = new RemoteInput
            {
                ComputerId = _computerId,
                Type = InputType.MouseClick,
                X = x,
                Y = y,
                Button = button
            };
            
            await SendCommandAsync(command);
        }
        
        // Отправка команды нажатия клавиши
        public async Task SendKeyPressAsync(int keyCode)
        {
            if (!_isConnected)
                return;
            
            var command = new RemoteInput
            {
                ComputerId = _computerId,
                Type = InputType.KeyPress,
                KeyCode = keyCode
            };
            
            await SendCommandAsync(command);
        }
        
        // Общий метод отправки команды
        public async Task SendCommandAsync(RemoteInput command)
        {
            try
            {
                await _httpClient.PostAsJsonAsync($"{BaseUrl}/remotecontrol/{_computerId}/public-command", command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке команды: {ex.Message}");
            }
        }
    }
} 
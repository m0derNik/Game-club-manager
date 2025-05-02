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
    public class RemoteDesktopService
    {
        private static RemoteDesktopService _instance;
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private Timer _refreshTimer;
        private int _computerId;
        private bool _isConnected;
        
        private const string BaseUrl = "http://localhost:7001/api";
        private const int RefreshInterval = 100; // 10 кадров в секунду
        
        // Делегат для обновления изображения
        public delegate void ScreenshotUpdatedEventHandler(BitmapImage screenshot);
        public event ScreenshotUpdatedEventHandler ScreenshotUpdated;
        
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
                var response = await _httpClient.PostAsync($"{BaseUrl}/remotecontrol/{computerId}/start", null);
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"Ошибка при подключении к компьютеру: {response.StatusCode}", 
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
                MessageBox.Show($"Ошибка при подключении к компьютеру: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        
        // Отключение от удаленного компьютера
        public async Task DisconnectAsync()
        {
            try
            {
                if (!_isConnected)
                    return;
                
                StopScreenshotUpdates();
                
                // Останавливаем сессию удаленного управления
                await _httpClient.PostAsync($"{BaseUrl}/remotecontrol/{_computerId}/stop", null);
                
                _isConnected = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отключении от компьютера: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Запуск таймера обновления скриншотов
        private void StartScreenshotUpdates()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Запускаем таймер обновлений
            _refreshTimer = new Timer(async _ => await UpdateScreenshotAsync(), 
                null, 0, RefreshInterval);
        }
        
        // Остановка таймера обновления скриншотов
        private void StopScreenshotUpdates()
        {
            _refreshTimer?.Dispose();
            _cancellationTokenSource?.Cancel();
        }
        
        // Получение и обработка скриншота
        private async Task UpdateScreenshotAsync()
        {
            try
            {
                if (!_isConnected)
                    return;
                
                var response = await _httpClient.GetAsync($"{BaseUrl}/remotecontrol/{_computerId}/screenshot");
                if (response.IsSuccessStatusCode)
                {
                    var screenshotData = await response.Content.ReadFromJsonAsync<RemoteDesktopData>();
                    if (screenshotData != null && screenshotData.ScreenData != null)
                    {
                        var bitmap = DecodeScreenshot(screenshotData);
                        if (bitmap != null)
                        {
                            // Вызываем событие с новым скриншотом
                            ScreenshotUpdated?.Invoke(bitmap);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Игнорируем отмененные задачи
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении скриншота: {ex.Message}");
            }
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
                await _httpClient.PostAsJsonAsync($"{BaseUrl}/remotecontrol/{_computerId}/command", command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке команды: {ex.Message}");
            }
        }
    }
} 
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using GameClubManager.Admin.Services;
using Microsoft.Win32;

namespace GameClubManager.Admin.Pages
{
    public partial class RemoteDesktopWindow : Window
    {
        private readonly RemoteDesktopService _remoteDesktopService;
        private readonly int _computerId;
        private readonly string _computerName;
        private double _scaleX = 1.0;
        private double _scaleY = 1.0;
        private Stopwatch _fpsStopwatch;
        private int _frameCount;
        private int _currentFps;
        
        public RemoteDesktopWindow(int computerId, string computerName)
        {
            InitializeComponent();
            
            _computerId = computerId;
            _computerName = computerName;
            _remoteDesktopService = RemoteDesktopService.Instance;
            
            // Настраиваем заголовок окна и отображение имени компьютера
            Title = $"Удаленное управление - {computerName}";
            ComputerNameText.Text = $"Компьютер: {computerName} (ID: {computerId})";
            
            // Подписываемся на обновления скриншотов
            _remoteDesktopService.ScreenshotUpdated += OnScreenshotUpdated;
            
            // Инициализируем счетчик FPS
            _fpsStopwatch = new Stopwatch();
            _fpsStopwatch.Start();
            
            // Фокусируем элемент изображения для корректной работы событий клавиатуры
            Loaded += (s, e) => 
            {
                RemoteDesktopImage.Focus();
                ConnectToComputer();
            };
        }
        
        // Подключение к компьютеру
        private async void ConnectToComputer()
        {
            ConnectionStatusText.Text = "Подключение...";
            ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Yellow;
            
            var result = await _remoteDesktopService.ConnectAsync(_computerId);
            
            if (result)
            {
                ConnectionStatusText.Text = "Подключено";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                ConnectionStatusText.Text = "Ошибка подключения";
                ConnectionStatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
        
        // Обработка события обновления скриншота
        private void OnScreenshotUpdated(BitmapImage screenshot)
        {
            // Вызываем метод в UI-потоке
            Dispatcher.Invoke(() =>
            {
                if (screenshot != null)
                {
                    // Обновляем изображение
                    RemoteDesktopImage.Source = screenshot;
                    
                    // Обновляем информацию о разрешении
                    ResolutionText.Text = $"{screenshot.PixelWidth}x{screenshot.PixelHeight}";
                    
                    // Вычисляем масштаб для преобразования координат
                    _scaleX = screenshot.PixelWidth / RemoteDesktopImage.ActualWidth;
                    _scaleY = screenshot.PixelHeight / RemoteDesktopImage.ActualHeight;
                    
                    // Обновляем счетчик FPS
                    _frameCount++;
                    if (_fpsStopwatch.ElapsedMilliseconds >= 1000)
                    {
                        _currentFps = _frameCount;
                        FpsText.Text = _currentFps.ToString();
                        _frameCount = 0;
                        _fpsStopwatch.Restart();
                    }
                }
            });
        }
        
        // Обработка событий мыши
        private async void RemoteDesktopImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_remoteDesktopService.IsConnected)
            {
                var position = e.GetPosition(RemoteDesktopImage);
                int remoteX = (int)(position.X * _scaleX);
                int remoteY = (int)(position.Y * _scaleY);
                
                // Обновляем отображение координат мыши
                MouseXText.Text = remoteX.ToString();
                MouseYText.Text = remoteY.ToString();
                
                // Отправляем команду движения мыши
                await _remoteDesktopService.SendMouseMoveAsync(remoteX, remoteY);
            }
        }
        
        private async void RemoteDesktopImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RemoteDesktopImage.Focus(); // Фокусируем для работы клавиатуры
            
            if (_remoteDesktopService.IsConnected)
            {
                var position = e.GetPosition(RemoteDesktopImage);
                int remoteX = (int)(position.X * _scaleX);
                int remoteY = (int)(position.Y * _scaleY);
                
                int button = 0;
                if (e.ChangedButton == MouseButton.Left)
                    button = 0;
                else if (e.ChangedButton == MouseButton.Right)
                    button = 1;
                else if (e.ChangedButton == MouseButton.Middle)
                    button = 2;
                
                // Отправляем команду нажатия кнопки мыши
                await _remoteDesktopService.SendMouseClickAsync(remoteX, remoteY, button);
            }
        }
        
        private async void RemoteDesktopImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Обработка отпускания кнопки мыши, если необходимо
        }
        
        private async void RemoteDesktopImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_remoteDesktopService.IsConnected)
            {
                var position = e.GetPosition(RemoteDesktopImage);
                int remoteX = (int)(position.X * _scaleX);
                int remoteY = (int)(position.Y * _scaleY);
                
                // Отправляем команду колеса мыши
                var command = new GameClubManager.Shared.Models.RemoteInput
                {
                    ComputerId = _computerId,
                    Type = GameClubManager.Shared.Models.InputType.MouseWheel,
                    X = remoteX,
                    Y = remoteY,
                    KeyCode = e.Delta
                };
                
                await _remoteDesktopService.SendCommandAsync(command);
            }
        }
        
        // Обработка событий клавиатуры
        private async void RemoteDesktopImage_KeyDown(object sender, KeyEventArgs e)
        {
            if (_remoteDesktopService.IsConnected)
            {
                // Преобразуем код клавиши WPF в виртуальный код клавиши Windows
                int virtualKeyCode = KeyInterop.VirtualKeyFromKey(e.Key);
                
                // Отправляем команду нажатия клавиши
                await _remoteDesktopService.SendKeyPressAsync(virtualKeyCode);
                
                // Предотвращаем стандартную обработку для специальных клавиш
                if (e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right || 
                    e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            }
        }
        
        private async void RemoteDesktopImage_KeyUp(object sender, KeyEventArgs e)
        {
            // Обработка отпускания клавиш, если необходимо
        }
        
        // Обработка нажатия кнопки Ctrl+Alt+Del
        private async void CtrlAltDelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_remoteDesktopService.IsConnected)
            {
                // Отправляем последовательность клавиш Ctrl+Alt+Del
                await _remoteDesktopService.SendKeyPressAsync(0x11); // Ctrl
                await _remoteDesktopService.SendKeyPressAsync(0x12); // Alt
                await _remoteDesktopService.SendKeyPressAsync(0x2E); // Delete
            }
        }
        
        // Отправка файла на удаленный компьютер - заглушка, будет реализовано позже
        private void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Выберите файл для отправки";
            dialog.Multiselect = false;
            
            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show($"Отправка файлов будет реализована в следующей версии.", 
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        // Отключение от удаленного компьютера
        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            await _remoteDesktopService.DisconnectAsync();
            Close();
        }
        
        // Обработка закрытия окна
        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Отписываемся от события обновления скриншотов
            _remoteDesktopService.ScreenshotUpdated -= OnScreenshotUpdated;
            
            // Отключаемся от удаленного компьютера
            await _remoteDesktopService.DisconnectAsync();
        }
    }
} 
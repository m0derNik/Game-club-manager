using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameClubManager.Client.Models
{
    public class Game
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("executablePath")]
        public string ExecutablePath { get; set; } = string.Empty;

        [JsonPropertyName("genre")]
        public int Genre { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; } = true;

        [JsonIgnore]
        private ImageSource _iconSource;

        [JsonIgnore]
        public ImageSource IconSource
        {
            get
            {
                if (_iconSource == null && !string.IsNullOrEmpty(ExecutablePath) && File.Exists(ExecutablePath))
                {
                    try
                    {
                        _iconSource = ExtractIconFromExe(ExecutablePath);
                    }
                    catch (Exception)
                    {
                        _iconSource = GetDefaultIcon();
                    }
                }
                else if (_iconSource == null)
                {
                    _iconSource = GetDefaultIcon();
                }

                return _iconSource;
            }
        }

        // Извлечение иконки из EXE файла
        private static ImageSource ExtractIconFromExe(string exePath)
        {
            try
            {
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    return GetDefaultIcon();
                }

                // Получаем иконку файла с помощью API Windows
                Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon != null)
                {
                    // Конвертируем иконку в BitmapSource для WPF
                    return Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
            catch (Exception ex)
            {
                // Логирование исключения
                Console.WriteLine($"Ошибка при извлечении иконки: {ex.Message}");
            }

            return GetDefaultIcon();
        }

        // Получение иконки по умолчанию
        private static ImageSource GetDefaultIcon()
        {
            try
            {
                string defaultIconPath = "pack://application:,,,/Resources/Images/default_game_icon.png";
                return new BitmapImage(new Uri(defaultIconPath, UriKind.Absolute));
            }
            catch
            {
                // Если не удалось загрузить иконку по умолчанию, возвращаем null
                return null;
            }
        }
    }
} 



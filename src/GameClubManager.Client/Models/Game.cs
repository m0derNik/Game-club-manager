using System.Text.Json.Serialization;

namespace GameClubManager.Client.Models
{
    public class Game
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("executablePath")]
        public string ExecutablePath { get; set; } = string.Empty;

        [JsonPropertyName("iconPath")]
        public string IconPath { get; set; } = string.Empty;

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = string.Empty;

        [JsonPropertyName("workingDirectory")]
        public string WorkingDirectory { get; set; } = string.Empty;

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; } = true;
    }
} 
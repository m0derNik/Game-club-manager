using System.Text.Json.Serialization;

namespace GameClubManager.Client.Models;

public class GameConfig
{
    [JsonPropertyName("games")]
    public List<Game> Games { get; set; } = new();

    [JsonPropertyName("adminPassword")]
    public string AdminPassword { get; set; } = string.Empty;

    [JsonPropertyName("serverUrl")]
    public string ServerUrl { get; set; } = string.Empty;
} 
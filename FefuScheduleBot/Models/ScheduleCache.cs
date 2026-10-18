using System.Text.Json.Serialization;

namespace FefuScheduleBot.Models;

[Serializable]
public class ScheduleCache
{
    [JsonPropertyName("fileId")]
    public string FileId { get; set; }
    
    [JsonPropertyName("hash")]
    public string Hash { get; set; }
}
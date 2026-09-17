using System.Text.Json.Serialization;

namespace FefuScheduleBot.Data;

[Serializable]
public class FefuScheduleData
{
    [JsonPropertyName("events")] 
    public FefuEvent[] Events { get; set; } = null!;
    
    [JsonPropertyName("subgroups")] 
    public string[] Subgroups { get; set; } = null!;
}
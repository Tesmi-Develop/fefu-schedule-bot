using System.Text.Json.Serialization;

namespace FefuScheduleBot.Data;

[Serializable]
public struct FefuScheduleData
{
    [JsonPropertyName("events")]
    public FefuEvent[] Events { get; set; }
}
using System.Text.Json.Serialization;

namespace FefuScheduleBot.Data;

[Serializable]
public class Config
{
    [JsonPropertyName("commonDisciplines")]
    public required int[] CommonDisciplines { get; set; }
}
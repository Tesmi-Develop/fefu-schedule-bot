using System.Text.Json.Serialization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace FefuScheduleBot.Data;

[Serializable]
public class FefuEvent
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("start")]
    public DateTime Start { get; set; }
    
    [JsonPropertyName("end")]
    public DateTime End { get; set; }
    
    [JsonPropertyName("disciplineId")]
    public int DisciplineId { get; set; }
    
    [JsonPropertyName("classroom")]
    public string Classroom { get; set; }
    
    [JsonPropertyName("pps_load")]
    public string PpsLoad { get; set; }
    
    [JsonPropertyName("teacher")]
    public string Teacher { get; set; }
    
    [JsonPropertyName("subgroup")]
    public string Subgroup { get; set; }
}
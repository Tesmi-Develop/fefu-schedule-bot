using System.Drawing;
using System.Text.Json.Serialization;

namespace FefuScheduleBot.Data;

[Serializable]
public class Config
{
    [JsonPropertyName("commonDisciplines")]
    public required int[] CommonDisciplines { get; set; }
    
    [JsonPropertyName("disciplines")]
    public required Dictionary<int, string> Disciplines { get; set; }
    
    [JsonPropertyName("typeDisciplinesByColors")]
    public required Dictionary<string, Color> TypeLessonByColors { get; set; }
    
    [JsonPropertyName("abbreviationTypeLessons")]
    public required Dictionary<string, string> AbbreviationTypeLessons { get; set; }
    
    [JsonPropertyName("countSubgroups")]
    public required int CountSubgroups { get; set; }
} 
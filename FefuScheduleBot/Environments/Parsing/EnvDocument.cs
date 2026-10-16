namespace FefuScheduleBot.Environments.Parsing;

public sealed class EnvEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public int SpacingBefore { get; set; }
}

public sealed class EnvDocument
{
    public List<EnvEntry> Entries { get; } = new();

    public Dictionary<string, string> ToDictionary()
    {
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in Entries)
        {
            if (string.IsNullOrEmpty(entry.Key))
                continue;

            dictionary[entry.Key] = entry.Value;
        }

        return dictionary;
    }
}
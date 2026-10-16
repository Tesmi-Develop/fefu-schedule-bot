namespace FefuScheduleBot.Environments.Parsing;

public enum EnvTokenType
{
    Comment,
    Key,
    Equals,
    Value,
    NewLine,
    Eof
}

public readonly struct EnvToken
{
    public EnvTokenType Type { get; }
    public string Value { get; }
    public int Line { get; }

    public EnvToken(EnvTokenType type, string value, int line)
    {
        Type = type;
        Value = value;
        Line = line;
    }
}
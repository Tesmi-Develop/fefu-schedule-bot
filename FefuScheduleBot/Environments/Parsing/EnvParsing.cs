namespace FefuScheduleBot.Environments.Parsing;

public sealed class EnvParser
{
    private readonly IReadOnlyList<EnvToken> _tokens;
    private int _position;

    public EnvParser(IReadOnlyList<EnvToken> tokens)
    {
        _tokens = tokens;
    }

    public EnvDocument Parse()
    {
        var document = new EnvDocument();
        var pendingSpacing = 0;
        string? pendingComment = null;

        while (!IsAtEnd())
        {
            var token = Peek();

            switch (token.Type)
            {
                case EnvTokenType.NewLine:
                    pendingSpacing++;
                    _position++;
                    continue;
                case EnvTokenType.Comment:
                    pendingComment = token.Value;
                    _position++;
                    SkipNewLine();
                    continue;
                case EnvTokenType.Key:
                {
                    var entry = ParseEntry(pendingSpacing, pendingComment);
                    if (entry != null)
                        document.Entries.Add(entry);

                    pendingSpacing = 0;
                    pendingComment = null;
                    continue;
                }
                default:
                    _position++;
                    break;
            }
        }

        return document;
    }

    private EnvEntry? ParseEntry(int spacingBefore, string? comment)
    {
        var keyToken = Consume(EnvTokenType.Key);
        if (keyToken == null)
            return null;

        if (Consume(EnvTokenType.Equals) == null)
            return null;

        var valueToken = Peek();
        var value = string.Empty;

        if (valueToken.Type is EnvTokenType.Key or EnvTokenType.Value)
        {
            value = valueToken.Value;
            _position++;
        }

        SkipNewLine();

        return new EnvEntry
        {
            Key = keyToken.Value.Value,
            Value = value,
            Comment = comment,
            SpacingBefore = spacingBefore
        };
    }

    private EnvToken? Consume(EnvTokenType type)
    {
        var token = Peek();
        if (token.Type != type)
            return null;

        _position++;
        return token;
    }

    private void SkipNewLine()
    {
        if (Peek().Type == EnvTokenType.NewLine)
            _position++;
    }

    private EnvToken Peek() => IsAtEnd() ? _tokens[^1] : _tokens[_position];
    private bool IsAtEnd() => _position >= _tokens.Count || _tokens[_position].Type == EnvTokenType.Eof;
}
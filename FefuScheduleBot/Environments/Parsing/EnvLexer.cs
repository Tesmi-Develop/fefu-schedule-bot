namespace FefuScheduleBot.Environments.Parsing;

public sealed class EnvLexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;

    public EnvLexer(string source)
    {
        _source = source;
    }

    public List<EnvToken> Tokenize()
    {
        var tokens = new List<EnvToken>();

        while (!IsAtEnd())
        {
            var token = ReadNextToken();
            if (token.Type == EnvTokenType.Eof)
                break;

            tokens.Add(token);
        }

        tokens.Add(new EnvToken(EnvTokenType.Eof, string.Empty, _line));
        return tokens;
    }

    private EnvToken ReadNextToken()
    {
        SkipSpaces();

        if (IsAtEnd())
            return new EnvToken(EnvTokenType.Eof, string.Empty, _line);

        var currentChar = Peek();

        if (currentChar is '\r' or '\n')
            return ReadNewLine();

        if (currentChar == '#')
            return ReadComment();

        if (currentChar == '=')
        {
            _position++;
            return new EnvToken(EnvTokenType.Equals, "=", _line);
        }

        return ReadKeyOrValue();
    }

    private EnvToken ReadComment()
    {
        _position++;

        if (Peek() == ' ')
            _position++;

        var start = _position;
        while (!IsAtEnd() && Peek() != '\r' && Peek() != '\n')
        {
            _position++;
        }

        var content = _source[start.._position];
        return new EnvToken(EnvTokenType.Comment, content, _line);
    }

    private EnvToken ReadKeyOrValue()
    {
        var currentChar = Peek();

        if (currentChar is '"' or '\'')
            return ReadQuotedValue(currentChar);

        var start = _position;
        while (!IsAtEnd())
        {
            var ch = Peek();
            if (char.IsWhiteSpace(ch) || ch is '=' or '#' or '\r' or '\n')
                break;

            _position++;
        }

        var value = _source[start.._position];
        return new EnvToken(EnvTokenType.Key, value, _line);
    }

    private EnvToken ReadQuotedValue(char quoteChar)
    {
        _position++;
        var start = _position;

        while (!IsAtEnd() && Peek() != quoteChar)
        {
            if (Peek() == '\\' && _position + 1 < _source.Length)
                _position++;

            _position++;
        }

        var value = _source[start.._position];

        if (!IsAtEnd())
            _position++;

        return new EnvToken(EnvTokenType.Value, value, _line);
    }

    private EnvToken ReadNewLine()
    {
        if (Peek() == '\r')
            _position++;

        if (Peek() == '\n')
            _position++;

        var token = new EnvToken(EnvTokenType.NewLine, Environment.NewLine, _line);
        _line++;
        return token;
    }

    private void SkipSpaces()
    {
        while (!IsAtEnd())
        {
            var ch = Peek();
            if (ch is ' ' or '\t')
            {
                _position++;
                continue;
            }

            break;
        }
    }

    private char Peek() => IsAtEnd() ? '\0' : _source[_position];
    private bool IsAtEnd() => _position >= _source.Length;
}
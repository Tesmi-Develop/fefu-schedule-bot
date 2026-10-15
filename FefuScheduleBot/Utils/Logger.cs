using System.Text;
using Hypercube.Utilities.Constants;
using Hypercube.Utilities.Debugging.Logger;

namespace FefuScheduleBot.Utils;

public sealed class MyLogger : ConsoleLogger
{
    private readonly string _dateTimeFormat;
    private readonly TimeZoneInfo _targetTimeZone;
    private readonly Type? _type;

    public MyLogger(string timeZoneId = "UTC", string dateTimeFormat = "yyyy-MM-dd HH:mm:ss")
    {
        _targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        _dateTimeFormat = dateTimeFormat;
    }
    
    public MyLogger(object target, string timeZoneId = "UTC", string dateTimeFormat = "yyyy-MM-dd HH:mm:ss")
    {
        _type = target?.GetType();
        _targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        _dateTimeFormat = dateTimeFormat;
    }
    
    /// <inheritdoc/>
    public override void Log(LogLevel level, string message)
    {
        if (level < LogLevel)
            return;

        var color = GetLineColor(level);
        
        var lines = message.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        if (lines.Length == 0)
            return;

        var sb = new StringBuilder();
        
        var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _targetTimeZone);
        var formattedTime = localTime.ToString(_dateTimeFormat);
        
        sb.Append($"[{formattedTime}] {color}[{level}]{Ansi.Reset}{(_type is not null ? $" {_type.Name}:" : string.Empty)} {lines[0]}{Ansi.Reset}");
        
        var secondaryPrefix = $" {color}>{Ansi.Reset} ";

        for (var i = 1; i < lines.Length; i++)
        {
            sb.Append('\n');
            sb.Append($"{secondaryPrefix}{lines[i]}{Ansi.Reset}");
        }

        Echo(sb.ToString());
    }

    /// <inheritdoc/>
    public override void Log(LogLevel level, Exception exception, string message = "")
    {
        var fullMessage = string.IsNullOrWhiteSpace(message) 
            ? exception.ToString() 
            : $"{message}\n{exception}";

        Log(level, fullMessage);
    }

    /// <inheritdoc/>
    public string GetLineColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => Ansi.BrightBlack,
            LogLevel.Debug => Ansi.Magenta,
            LogLevel.Info => Ansi.Green,
            LogLevel.Warning => Ansi.Yellow,
            LogLevel.Error => Ansi.Red,
            LogLevel.Critical => Ansi.BrightRed,
            _ => Ansi.Reset
        };
    }
}
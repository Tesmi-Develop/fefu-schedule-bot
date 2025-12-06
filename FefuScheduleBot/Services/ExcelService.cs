using System.Drawing;
using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Dependencies;
using Hypercube.Mathematics.Vectors;
using Spire.Xls;
using Style = FefuScheduleBot.Classes.Style;

namespace FefuScheduleBot.Services;

[Service]
public class ExcelService
{
    private readonly int _width = Schedule.CountWorkingDays + 2;
    [Dependency] private readonly Config _config = null!;
    [Dependency] private readonly FefuService _fefuService = null!;

    public void SaveTableToFile(Worksheet worksheet, string name = "")
    {
        var fileName = name == string.Empty
            ? $"Расписание {_fefuService.GetLocalTime().ToStringWithCulture("d")}.xlsx"
            : $"{name}.xlsx";
        
        if (File.Exists(fileName))
            File.Delete(fileName);
        
        worksheet.SaveToFile(fileName, string.Empty);
    }
    
    public MemoryStream GenerateStreamTable(Schedule schedule)
    {
        var startPosition = new Vector2i(1, 1);
        using var workbook = new Workbook();
        var sheet = workbook.Worksheets[0];
        
        AddLabels(sheet, ref startPosition);
        AddDescription(sheet, ref startPosition);

        var firstPoint = startPosition;
        AddLefSide(sheet, ref startPosition);
        AddHat(sheet, ref startPosition, schedule);
        var endPosition = FillTable(sheet, ref startPosition, schedule, schedule.Week);
        
        SetBorder(sheet.Range[firstPoint.Y, firstPoint.X, endPosition.Y, endPosition.X]);
        
        sheet.Range.AutoFitColumns();
        sheet.Range.AutoFitRows();
        
        var stream = new MemoryStream();
        workbook.SaveToStream(stream, FileFormat.Version2016);
        return stream;
    }

    private void ApplyStyle(CellRange range, Style style)
    {
        range.Style.HorizontalAlignment = style.HorizontalAlignment;
        range.Style.Font.IsBold = style.TextBold;
        range.Style.Font.Color = style.TextColor;
    }
    
    private void AddDescription(Worksheet sheet, ref Vector2i startPosition)
    {
        var range = sheet.Range[startPosition.Y, startPosition.X, startPosition.Y, Schedule.CountWorkingDays + 2];
        
        range.Merge();
        range.Value = "Сгенерировано автоматически (автор Tesmi)";
        ApplyStyle(range, Style.Hat);
        
        startPosition += new Vector2i(0, 1);
    }

    private void AddLabels(Worksheet sheet, ref Vector2i startPosition)
    {
        var offset = 2;
        var firstPoint = startPosition;
        
        foreach (var (type, color) in _config.TypeLessonByColors)
        {
            var abbreviation = _config.AbbreviationTypeLessons.GetValueOrDefault(type, type);
            var range = sheet.Range[startPosition.Y, startPosition.X + offset, startPosition.Y,
                startPosition.X + offset];

            AddText(range, abbreviation, Style.Default);
            range.Style.Color = color;
            
            offset += 1;
        }

        var endPoint = new Vector2i(firstPoint.X + _width - 1, firstPoint.Y);
        SetBorder(sheet.Range[firstPoint.Y, firstPoint.X, endPoint.Y, endPoint.X]);
        
        startPosition += new Vector2i(0, 1);
    }

    private void SetBorder(CellRange range)
    {
        range.Style.Borders[BordersLineType.EdgeTop].LineStyle = LineStyleType.Thin;
        range.Style.Borders[BordersLineType.EdgeBottom].LineStyle = LineStyleType.Thin;
        range.Style.Borders[BordersLineType.EdgeLeft].LineStyle = LineStyleType.Thin;
        range.Style.Borders[BordersLineType.EdgeRight].LineStyle = LineStyleType.Thin;
    }
        
    private Vector2i FillTable(Worksheet sheet, ref Vector2i position, Schedule content, Week week)
    {
        var startPosition = position;
        var origY = position.Y;
        
        foreach (var (day, times) in content.Days)
        {
            position = position.WithY(origY);
            
            foreach (var (_, events) in times)
            {
                var currentY = position.Y;
                position += Vector2i.UnitY;
                
                if (events.Count == 0)
                    continue;
                
                var @event = events[0];
                var disciplineName =
                    _config.Disciplines.TryGetValue(@event.DisciplineId, out var name) ? name : @event.Title;
                var classroom = @event.Classroom != string.Empty ? $" | {@event.Classroom}" : string.Empty;
                var title = $"{disciplineName}{classroom}";
                var range = sheet[currentY, position.X];
                var color = _config.TypeLessonByColors.GetValueOrDefault(@event.PpsLoad, Color.Azure);
                
                AddText(range, title, Style.DefaultWithoutBold);
                range.Style.Color = color;
            }

            position += Vector2i.UnitX;
        }

        var endPosition = position - new Vector2i(1, 1);
        position = startPosition;
        
        return endPosition;
    }

    private void AddHat(Worksheet sheet, ref Vector2i startPosition, Schedule schedule)
    {
        var point = startPosition;
        foreach (var (day, _) in schedule.Days)
        {
            var range = sheet[point.Y, point.X];
            AddText(range, day.ToStringWithCulture("d"), Style.Default);
            point += new Vector2i(1, 0);
        }
        
        startPosition += new Vector2i(0, 1);
    }

    private void AddText(CellRange range, string text, Style? style = null)
    {
        range.Style.NumberFormat = "@";
        range.Text = text;

        if (style is not null)
            ApplyStyle(range, (Style) style);
    }
    
    private void AddLefSide(Worksheet sheet, ref Vector2i startPosition)
    {
        AddText(sheet[startPosition.Y, startPosition.X], "Пары", Style.Default);
        AddText(sheet[startPosition.Y, startPosition.X + 1], "Время", Style.Default);

        var i = 1;
        foreach (var hashedTime in Schedule.HashedLessonTimes)
        {
            AddText(sheet[startPosition.Y + i, startPosition.X], $"{i} Пара", Style.Default);
            AddText(sheet[startPosition.Y + i, startPosition.X + 1], hashedTime, Style.Default);
            i++;
        }
        
        startPosition += new Vector2i(2, 0);
    }
}
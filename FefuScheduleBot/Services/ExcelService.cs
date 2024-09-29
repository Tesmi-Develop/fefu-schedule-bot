using System.Drawing;
using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Dependencies;
using Hypercube.Mathematics.Vectors;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace FefuScheduleBot.Services;

public enum WeekType
{
    Current,
    Next
}

[Service]
public class ExcelService
{
    private readonly Dictionary<int, string> _abbreviations = new()
    {
        { 5297, "МАТ. АНАЛИЗ" },
        { 28660, "ОСН. ЭКН. ГРАМ" },
        { 13933, "ФИЗ-РА" },
        { 6672, "ЛИН. АЛГЕБРА" },
        { 5358, "ИСТОРИЯ. Р" },
        { 28661, "РУССКИЙ" },
        { 30210, "ОСН. Р. ГОС" },
        { 15351, "ИН. ЯЗЫК" },
        { 9925, "ОСН. АЛГ. И ПРОГ" },
        { 19986, "ОСН. ЦИФ. ГРАМ" },
        { 34077, "CОЦ-ПСИХ. ТЕСТ" },
    };

    private readonly Dictionary<string, string> _abbreviationTypeLessons = new()
    {
        {"Лабораторные работы", "Лабораторная"},
        {"Практические занятия", "Практическая"},
        {"Лекционные занятия", "Лекция"}
    };
    
    private readonly Dictionary<string, Color> _typeLessonByColors = new()
    {
        {"Лекционные занятия", Color.FromArgb(244, 176, 132)},
        {"Лабораторные работы", Color.FromArgb(255, 192, 0)},
        {"Практические занятия", Color.FromArgb(85, 151, 211)},
        {"Мероприятие", Color.FromArgb(146, 208, 80)}
    };

    private readonly int _width = Calendar.CountWorkingDays + 2;
    
    [Dependency] private readonly FefuService _fefuService = default!;

    public async Task<ExcelPackage> GenerateSchedule(WeekType weekType, int subgroup)
    {
        var time = _fefuService.GetLocalTime();
        var fileName = $"Расписание {time.ToStringWithCulture("d")}";
        var week = weekType == WeekType.Current
            ? _fefuService.GetStudyWeek() 
            : _fefuService.GetStudyWeek(_fefuService.GetLocalTime().AddDays(7));
        
        var events = await _fefuService.GetEvents(week.Start, week.End);
        var calendar = new Calendar(events ?? []).UseSubgroup(subgroup);

        return GenerateTable(new FileInfo($"{fileName}.xlsx"), calendar, week);
    }

    private ExcelPackage GenerateTable(FileInfo file, Calendar calendar, Week week)
    {
        var startPosition = new Vector2i(1, 1);
        var excel = new ExcelPackage(file);
        var sheet = excel.Workbook.Worksheets.Add("Лист 1");
        
        AddLabels(sheet, ref startPosition);
        AddDescription(sheet, ref startPosition);

        var firstPoint = startPosition;
        AddLefSide(sheet, ref startPosition);
        AddHat(sheet, ref startPosition, week);
        FillTable(sheet, ref startPosition, calendar);
        
        SetBorder(sheet.Cells[firstPoint.Y, firstPoint.X, startPosition.Y, startPosition.X]);
        
        sheet.Cells.AutoFitColumns();
        excel.Save();
        
        return excel;
    }

    private void ApplyStyle(ExcelRange range, Style style)
    {
        range.Style.HorizontalAlignment = style.HorizontalAlignment;
        range.Style.Font.Bold = style.TextBold;
        range.Style.Font.Color.SetColor(style.TextColor);
    }
    
    private void AddDescription(ExcelWorksheet sheet, ref Vector2i startPosition)
    {
        var range = sheet.Cells[startPosition.Y, startPosition.X, startPosition.Y, Calendar.CountWorkingDays + 2];
        
        range.Merge = true;
        range.Value = "Сгенерировано ботом (автор Tesmi)";
        ApplyStyle(range, Style.Hat);
        
        startPosition += new Vector2i(0, 1);
    }

    private void AddLabels(ExcelWorksheet sheet, ref Vector2i startPosition)
    {
        var offset = 2;
        var firstPoint = startPosition;
        
        foreach (var (type, color) in _typeLessonByColors)
        {
            var abbreviation = _abbreviationTypeLessons.GetValueOrDefault(type, type);
            var range = sheet.Cells[startPosition.Y, startPosition.X + offset, startPosition.Y,
                startPosition.X + offset];

            AddText(range, abbreviation, Style.Default);
            range.Style.Fill.SetBackground(color);
            
            offset += 1;
        }

        var endPoint = new Vector2i(firstPoint.X + _width - 1, firstPoint.Y);
        SetBorder(sheet.Cells[firstPoint.Y, firstPoint.X, endPoint.Y, endPoint.X]);
        
        startPosition += new Vector2i(0, 1);
    }

    private void SetBorder(ExcelRange range)
    {
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    }
        
    private void FillTable(ExcelWorksheet sheet, ref Vector2i startPosition, Calendar content)
    {
        var origY = startPosition.Y;
        
        foreach (var (_, times) in content.Days)
        {
            startPosition = startPosition.WithY(origY);
            
            foreach (var (_, events) in times)
            {
                var @event = events[0];
                var disciplineName =
                    _abbreviations.TryGetValue(@event.DisciplineId, out var name) ? name : @event.Title;
                var classroom = @event.Classroom != string.Empty ? $" | {@event.Classroom}" : string.Empty;
                var title = $"{disciplineName}{classroom}";
                var range = sheet.Cells[origY + (@event.Order - 1), startPosition.X];
                var color = _typeLessonByColors.GetValueOrDefault(@event.PpsLoad, Color.Azure);
                
                AddText(range, title, Style.DefaultWithoutBold);
                range.Style.Fill.SetBackground(color);
                
                startPosition += Vector2i.UnitY;
            }

            startPosition += Vector2i.UnitX;
        }
        
        startPosition -= Vector2i.UnitX;
        startPosition = startPosition.WithY(origY + Calendar.CountLessons - 1);
    }

    private void AddHat(ExcelWorksheet sheet, ref Vector2i startPosition, Week week)
    {
        var day = week.Start;
        var end = week.End.AddDays(1);
        var point = startPosition;
        
        while (day != end)
        {
            var range = sheet.Cells[point.Y, point.X];
            AddText(range, day.ToStringWithCulture("d"), Style.Default);
            
            day = day.AddDays(1);
            point += new Vector2i(1, 0);
        }
        
        startPosition += new Vector2i(0, 1);
    }

    private void AddText(ExcelRange range, string text, Style? style = default)
    {
        range.Value = text;

        if (style is not null)
        {
            ApplyStyle(range, (Style) style);
        }
    }
    
    private void AddLefSide(ExcelWorksheet sheet, ref Vector2i startPosition)
    {
        AddText(sheet.Cells[startPosition.Y, startPosition.X], "Пары:", Style.Default);
        AddText(sheet.Cells[startPosition.Y, startPosition.X + 1], "Время:", Style.Default);
        
        for (var i = 1; i <= Calendar.CountLessons; i++)
        {
            var hashedTime = Calendar.HashedLessonTimes[i - 1];
            
            AddText(sheet.Cells[startPosition.Y + i, startPosition.X], $"{i} Пара", Style.Default);
            AddText(sheet.Cells[startPosition.Y + i, startPosition.X + 1], hashedTime, Style.Default);
        }
        
        startPosition += new Vector2i(2, 0);
    }

    public void Init()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }
}
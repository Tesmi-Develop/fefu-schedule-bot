using System.Drawing;
using Aspose.Cells;
using Aspose.Cells.Drawing;
using Aspose.Cells.Rendering;
using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.ServiceRealisation;
using FefuScheduleBot.Utils.Extensions;
using Hypercube.Dependencies;
using Hypercube.Mathematics.Vectors;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Style = FefuScheduleBot.Classes.Style;

namespace FefuScheduleBot.Services;

public enum WeekType
{
    Current,
    Next
}

[Service]
public class ExcelService : IInitializable
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
        { 5279, "ДИСК. МАТ."},
        { 8945, "АНАЛИТ. ГЕОМЕТРИЯ"},
        { 5371, "ОПД"},
        { 65, "БЖД"},
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

    public async Task<FileInfo> GenerateSchedule(WeekType weekType, int subgroup, string customName = "")
    {
        var time = _fefuService.GetLocalTime();
        var fileName = $"Расписание {time.ToStringWithCulture("d")}";
        var week = weekType == WeekType.Current
            ? _fefuService.GetStudyWeek() 
            : _fefuService.GetStudyWeek(_fefuService.GetLocalTime().AddDays(7));
        
        var events = await _fefuService.GetEvents(week.Start, week.End);
        var calendar = new Calendar(events ?? []).UseSubgroup(subgroup);
        var fileInfo = new FileInfo(customName == string.Empty ? $"{fileName}.xlsx" : $"{customName}.xlsx");

        if (File.Exists(fileInfo.FullName))
            File.Delete(fileInfo.FullName);

        return GenerateTable(fileInfo, calendar, week).File;
    }

    public async Task<FileInfo> GenerateScheduleImage(WeekType weekType, int subgroup)
    {
        var time = _fefuService.GetLocalTime();
        var excel = await GenerateSchedule(weekType, subgroup, $"{time.ToStringWithCulture("d")}-table");
        var book = new Workbook(excel.OpenRead());
        var sheet = book.Worksheets[0];

        var imgOptions = new ImageOrPrintOptions
        {
            ImageType = ImageType.Jpeg,
            OnePagePerSheet = true,
        };

        var fileName = $"Расписание {time.ToStringWithCulture("d")}.jpeg";
        var render = new SheetRender(sheet, imgOptions);
        render.ToImage(0, fileName);
        
        return new FileInfo(fileName);
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
        FillTable(sheet, ref startPosition, calendar, week);
        
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
        
    private void FillTable(ExcelWorksheet sheet, ref Vector2i startPosition, Calendar content, Week week)
    {
        var origY = startPosition.Y;
        startPosition += new Vector2i((content.Days.Keys.First() - week.Start.Date).Days, 0);
        
        foreach (var (day, times) in content.Days)
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
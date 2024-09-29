using System.Drawing;
using OfficeOpenXml.Style;
namespace FefuScheduleBot.Classes;

public struct Style
{
    public static readonly Style Hat = new()
    {
        HorizontalAlignment = ExcelHorizontalAlignment.Center,
        TextBold = true,
        TextColor = Color.FromArgb(86,86,86),
    };
    
    public static readonly Style Default = new()
    {
        HorizontalAlignment = ExcelHorizontalAlignment.Center,
        TextBold = true,
        
        TextColor = Color.FromArgb(0, 0, 0),
    };
    
    public static readonly Style DefaultWithoutBold = new()
    {
        HorizontalAlignment = ExcelHorizontalAlignment.Center,
        TextBold = false,
        TextColor = Color.FromArgb(0, 0, 0),
    };

    public Style() {}

    public ExcelHorizontalAlignment HorizontalAlignment = ExcelHorizontalAlignment.Center;
    public bool TextBold = true;
    public Color TextColor = Color.Aqua;
}
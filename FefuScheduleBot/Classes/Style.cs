using System.Drawing;
using Spire.Xls;

namespace FefuScheduleBot.Classes;

public struct Style
{
    public static readonly Style Hat = new()
    {
        HorizontalAlignment =  HorizontalAlignType.Center,
        TextBold = true,
        TextColor = Color.FromArgb(86,86,86),
    };
    
    public static readonly Style Default = new()
    {
        HorizontalAlignment =  HorizontalAlignType.Center,
        TextBold = true,
        
        TextColor = Color.FromArgb(0, 0, 0),
    };
    
    public static readonly Style DefaultWithoutBold = new()
    {
        HorizontalAlignment =  HorizontalAlignType.Center,
        TextBold = false,
        TextColor = Color.FromArgb(0, 0, 0),
    };

    public Style() {}

    public HorizontalAlignType HorizontalAlignment = HorizontalAlignType.Center;
    public bool TextBold = true;
    public Color TextColor = Color.Aqua;
}
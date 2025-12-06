using FefuScheduleBot.ServiceRealisation;
using SkiaSharp;
using Spire.Xls;

namespace FefuScheduleBot.Services;

[Service]
public class ImageService
{
    public Stream GenerateStreamFromTable(Stream workbookStream)
    {
        using var workbook = new Workbook();
        workbook.LoadFromStream(workbookStream);
        var worksheet = workbook.Worksheets[0];
        return worksheet.ToImage(1, 1, worksheet.LastRow, worksheet.LastColumn);
    }

    public Stream ApplyBackground(Stream imageStream)
    {
        var originalBitmap = SKBitmap.Decode(imageStream);
        var outputStream = new MemoryStream();
        using var canvas = new SKCanvas(originalBitmap);
        using var textPaint = new SKPaint();
        
        textPaint.Color = SKColors.Black;
        textPaint.TextSize = 36.0f;
        textPaint.Typeface = SKTypeface.FromFamilyName("DejaVu Sans", SKTypefaceStyle.Bold); 
            
        textPaint.IsAntialias = true;
        canvas.DrawText("Hello", new SKPoint(100, 50), textPaint);

        var image = SKImage.FromBitmap(originalBitmap);
        var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
        encodedData.SaveTo(outputStream);

        outputStream.Position = 0;
        return outputStream;
    }
}
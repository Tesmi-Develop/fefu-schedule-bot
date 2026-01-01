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
        var anchorX = 0.5f;
        var anchorY = 0.5f;
        var padding = 60;
        var scheduleAnchorX = 0.5f; 
        var scheduleAnchorY = 0.5f;
        var scheduleScale = 1f;
        var backgroundScale = 0.7f;
        
        using var originalBitmap = SKBitmap.Decode(imageStream);
        
        using var backgroundFileStream = File.OpenRead("background.png");
        using var backgroundBitmap = SKBitmap.Decode(backgroundFileStream);
        
        var backgroundScaledWidth = backgroundBitmap.Width * backgroundScale;
        var backgroundScaledHeight = backgroundBitmap.Height * backgroundScale;
        
        var canvasWidth = (int)Math.Round(backgroundScaledWidth);
        var canvasHeight = (int)Math.Round(backgroundScaledHeight);
        
        using var resultBitmap = new SKBitmap(canvasWidth, canvasHeight);
        using var canvas = new SKCanvas(resultBitmap);
        
        var scheduleBitmap = originalBitmap;
        
        if (padding > 0)
        {
            var croppedWidth = Math.Max(1, originalBitmap.Width - 2 * padding);
            var croppedHeight = Math.Max(1, originalBitmap.Height - 2 * padding);
            
            if (croppedWidth < originalBitmap.Width && croppedHeight < originalBitmap.Height)
            {
                var croppedBitmap = new SKBitmap(croppedWidth, croppedHeight);
                
                using var croppedCanvas = new SKCanvas(croppedBitmap);
                var sourceRect = new SKRect(padding, padding, 
                    originalBitmap.Width - padding, originalBitmap.Height - padding);
                var destRect = new SKRect(0, 0, croppedWidth, croppedHeight);
                
                croppedCanvas.DrawBitmap(originalBitmap, sourceRect, destRect);
                scheduleBitmap = croppedBitmap;
            }
        }
        
        var scheduleScaledWidth = scheduleBitmap.Width * scheduleScale;
        var scheduleScaledHeight = scheduleBitmap.Height * scheduleScale;
        
        var targetX = anchorX * canvasWidth;
        var targetY = anchorY * canvasHeight;
        
        var scheduleX = targetX - scheduleAnchorX * scheduleScaledWidth;
        var scheduleY = targetY - scheduleAnchorY * scheduleScaledHeight;
        
        scheduleX = Math.Max(0, Math.Min(scheduleX, canvasWidth - scheduleScaledWidth));
        scheduleY = Math.Max(0, Math.Min(scheduleY, canvasHeight - scheduleScaledHeight));
        
        var backgroundDestRect = new SKRect(0, 0, canvasWidth, canvasHeight);
        
        var scheduleDestRect = new SKRect(scheduleX, scheduleY, 
            scheduleX + scheduleScaledWidth, scheduleY + scheduleScaledHeight);
        canvas.DrawBitmap(scheduleBitmap, scheduleDestRect);
        canvas.DrawBitmap(backgroundBitmap, backgroundDestRect);
        
        if (padding > 0 && scheduleBitmap != originalBitmap)
            scheduleBitmap.Dispose();
        
        var outputStream = new MemoryStream();
        var image = SKImage.FromBitmap(resultBitmap);
        var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
        encodedData.SaveTo(outputStream);
        
        outputStream.Position = 0;
        return outputStream;
    }
}
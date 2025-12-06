using FefuScheduleBot.ServiceRealisation;
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
        // TODO
        /*var originalBitmap = SKBitmap.Decode(imageStream);
        var outputStream = new MemoryStream();
        using var backgroundFileStream = File.OpenRead("background.png");
        using var backgroundBitmap = SKBitmap.Decode(backgroundFileStream);
        using var canvas = new SKCanvas(originalBitmap);
        
        var x = (originalBitmap.Width - backgroundBitmap.Width) / 2;
        var y = (originalBitmap.Height - backgroundBitmap.Width) / 2;
        
        var destRect = new SKRect(0, 0, originalBitmap.Width, originalBitmap.Height);
        canvas.DrawBitmap(backgroundBitmap, destRect);

        var image = SKImage.FromBitmap(originalBitmap);
        var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
        encodedData.SaveTo(outputStream);

        outputStream.Position = 0;*/
        return imageStream;
    }
}
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

        using var worksheet = workbook.Worksheets[0];
        return worksheet.ToImage(1, 1, worksheet.LastRow, worksheet.LastColumn);
    }

    public Stream ApplyBackground(Stream imageStream, DateTime dateTime)
    {
        using var originalBitmap = SKBitmap.Decode(imageStream);
        using var croppedBitmap = CropPadding(originalBitmap, 60);
        using var scheduleBitmap = CropWhiteMargins(croppedBitmap);

        return RenderCompositeImage(dateTime, scheduleBitmap);
    }

    private SKBitmap CropWhiteMargins(SKBitmap source, byte tolerance = 5)
    {
        var width = source.Width;
        var height = source.Height;

        var minX = width;
        var minY = height;
        var maxX = 0;
        var maxY = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = source.GetPixel(x, y);

                var isWhite = Math.Abs(color.Red - 255) <= tolerance &&
                              Math.Abs(color.Green - 255) <= tolerance &&
                              Math.Abs(color.Blue - 255) <= tolerance;

                if (!isWhite)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (minX >= maxX || minY >= maxY)
            return source.Copy();

        var cropWidth = maxX - minX + 1;
        var cropHeight = maxY - minY + 1;

        var resultBitmap = new SKBitmap(cropWidth, cropHeight);
        using var canvas = new SKCanvas(resultBitmap);

        var sourceRect = new SKRect(minX, minY, maxX + 1, maxY + 1);
        var destRect = new SKRect(0, 0, cropWidth, cropHeight);

        canvas.DrawBitmap(source, sourceRect, destRect);
        return resultBitmap;
    }

    private Stream RenderCompositeImage(DateTime dateTime, SKBitmap scheduleBitmap) => dateTime.Month switch
    {
        12 or 1 or 2 => RenderWinterBackground(dateTime, scheduleBitmap),
        3 or 4 or 5 => RenderSpringBackground(dateTime, scheduleBitmap),
        6 or 7 or 8 => RenderSummerBackground(dateTime, scheduleBitmap),
        _ => RenderAutumnBackground(dateTime, scheduleBitmap)
    };

    private Stream RenderAutumnBackground(DateTime dateTime, SKBitmap scheduleBitmap)
    {
        using var backBitmap = SKBitmap.Decode("Assets/Autumn/background.png");
        using var overlayBitmap = SKBitmap.Decode("Assets/Autumn/overlay.png");

        return RenderWithBackgrounds(
            scheduleBitmap: scheduleBitmap,
            backBitmap: backBitmap,
            overlayBitmap: overlayBitmap,
            backgroundScale: 0.7f,
            scheduleScale: 1.0f,
            anchorX: 0.5f,
            anchorY: 0.5f,
            scheduleAnchorX: 0.5f,
            scheduleAnchorY: 0.5f
        );
    }

    private Stream RenderWinterBackground(DateTime dateTime, SKBitmap scheduleBitmap)
    {
        using var backgroundBitmap = SKBitmap.Decode("Assets/Winter/background.png");

        return RenderWithBackgrounds(
            scheduleBitmap: scheduleBitmap,
            backBitmap: backgroundBitmap,
            overlayBitmap: null,
            backgroundScale: 0.7f,
            scheduleScale: 1.0f,
            anchorX: 0.5f,
            anchorY: 0.5f,
            scheduleAnchorX: 0.5f,
            scheduleAnchorY: 0.5f
        );
    }

    private Stream RenderSpringBackground(DateTime dateTime, SKBitmap scheduleBitmap)
    {
        using var backBitmap = SKBitmap.Decode("Assets/Spring/background.png");
        using var overlayBitmap = SKBitmap.Decode("Assets/Spring/overlay.png");

        return RenderWithBackgrounds(
            scheduleBitmap: scheduleBitmap,
            backBitmap: backBitmap,
            overlayBitmap: overlayBitmap,
            backgroundScale: 0.7f,
            scheduleScale: 1.0f,
            anchorX: 0.5f,
            anchorY: 0.5f,
            scheduleAnchorX: 0.5f,
            scheduleAnchorY: 0.5f
        );
    }

    private Stream RenderSummerBackground(DateTime dateTime, SKBitmap scheduleBitmap)
    {
        using var backBitmap = SKBitmap.Decode("Assets/Summer/background.png");
        using var overlayBitmap = SKBitmap.Decode("Assets/Summer/overlay.png");

        return RenderWithBackgrounds(
            scheduleBitmap: scheduleBitmap,
            backBitmap: backBitmap,
            overlayBitmap: overlayBitmap,
            backgroundScale: 0.7f,
            scheduleScale: 1.0f,
            anchorX: 0.5f,
            anchorY: 0.5f,
            scheduleAnchorX: 0.5f,
            scheduleAnchorY: 0.5f
        );
    }

    private Stream RenderWithBackgrounds(
        SKBitmap scheduleBitmap,
        SKBitmap backBitmap,
        SKBitmap? overlayBitmap,
        float backgroundScale,
        float scheduleScale,
        float anchorX,
        float anchorY,
        float scheduleAnchorX,
        float scheduleAnchorY)
    {
        var canvasWidth = (int)Math.Round(backBitmap.Width * backgroundScale);
        var canvasHeight = (int)Math.Round(backBitmap.Height * backgroundScale);

        using var resultBitmap = new SKBitmap(canvasWidth, canvasHeight);
        using var canvas = new SKCanvas(resultBitmap);

        var backgroundDestRect = new SKRect(0, 0, canvasWidth, canvasHeight);
        
        canvas.DrawBitmap(backBitmap, backgroundDestRect);
        
        var scheduleScaledWidth = scheduleBitmap.Width * scheduleScale;
        var scheduleScaledHeight = scheduleBitmap.Height * scheduleScale;

        var targetX = anchorX * canvasWidth;
        var targetY = anchorY * canvasHeight;

        var scheduleX = targetX - scheduleAnchorX * scheduleScaledWidth;
        var scheduleY = targetY - scheduleAnchorY * scheduleScaledHeight;

        scheduleX = Math.Max(0, Math.Min(scheduleX, canvasWidth - scheduleScaledWidth));
        scheduleY = Math.Max(0, Math.Min(scheduleY, canvasHeight - scheduleScaledHeight));

        var scheduleDestRect = new SKRect(
            scheduleX, 
            scheduleY, 
            scheduleX + scheduleScaledWidth, 
            scheduleY + scheduleScaledHeight
        );
        
        canvas.DrawBitmap(scheduleBitmap, scheduleDestRect);
        
        if (overlayBitmap != null)
            canvas.DrawBitmap(overlayBitmap, backgroundDestRect);

        return EncodeToPngStream(resultBitmap);
    }

    private SKBitmap CropPadding(SKBitmap source, int padding)
    {
        if (padding <= 0)
            return source.Copy();

        var croppedWidth = source.Width - 2 * padding;
        var croppedHeight = source.Height - 2 * padding;

        if (croppedWidth <= 0 || croppedHeight <= 0)
            return source.Copy();

        var croppedBitmap = new SKBitmap(croppedWidth, croppedHeight);
        using var canvas = new SKCanvas(croppedBitmap);

        var sourceRect = new SKRect(padding, padding, source.Width - padding, source.Height - padding);
        var destRect = new SKRect(0, 0, croppedWidth, croppedHeight);

        canvas.DrawBitmap(source, sourceRect, destRect);
        return croppedBitmap;
    }

    private Stream EncodeToPngStream(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var encodedData = image.Encode(SKEncodedImageFormat.Png, 100);

        var outputStream = new MemoryStream();
        encodedData.SaveTo(outputStream);
        outputStream.Position = 0;

        return outputStream;
    }
}
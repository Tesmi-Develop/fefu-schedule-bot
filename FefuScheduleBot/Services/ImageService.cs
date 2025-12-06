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
}
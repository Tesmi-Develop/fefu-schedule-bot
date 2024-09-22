using System.Text.Json;
using FefuScheduleBot.ServiceRealisation;
using BetterConsoles.Tables;
using BetterConsoles.Tables.Builders;
using BetterConsoles.Tables.Configuration;
using BetterConsoles.Tables.Models;
using FefuScheduleBot.Classes;
using FefuScheduleBot.Data;
using FefuScheduleBot.Environments;
using Hypercube.Dependencies;

namespace FefuScheduleBot.Services;

[Service]
public class FefuService : IStartable
{
    [Dependency] private readonly EnvironmentData _environmentData = default!;
    
    private const string Url = "https://univer.dvfu.ru/";
    private const string SheduleApi = "schedule/get";
    private readonly HttpClient _client = new();

    private async Task<FefuEvent[]?> GetSchedule()
    {
        var request = new HttpRequestMessage();
        request.RequestUri = new Uri($"{Url}{SheduleApi}?type=agendaWeek&start=2024-09-22&end=2024-09-28&groups[]=6534&ppsId=&facilityId=0");
        request.Method = HttpMethod.Get;

        request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:69.0) Gecko/20100101 Firefox/69.0");
        request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        request.Headers.Add("Cookie", $"_univer_identity={_environmentData.UniverId}; _jwts={_environmentData.Jwts}; LtpaToken2={_environmentData.LtpaToken}");
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();
        
        try
        {
            return JsonSerializer.Deserialize<FefuScheduleData>(content).Events;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }
    public async Task Start()
    {
        var data = await GetSchedule();
        if (data is null) return;

        var calendar = new Calendar(data).UseSubgroup(10);
        var columns = calendar.ToColumns((@event) => @event[0].Title);
        var maxHeight = columns[0].Count;
        var header = new List<Column>();

        foreach (var column in columns)
        {
            header.Add(new Column(column[0]));
        }
        
        var table = new Table(header.ToArray())
        {
            Config = TableConfig.Markdown()
        };

        for (var i = 1; i < maxHeight; i++)
        {
            var row = new List<Cell>();
            
            foreach (var column in columns)
            {
                row.Add(new Cell(column[i]));
            }

            table.AddRow(row.ToArray());
        }
        
        Console.WriteLine(table);
    }
}
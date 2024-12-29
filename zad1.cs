using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
class Program
{
    static async Task Delay(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
    private static readonly HttpClient client = new HttpClient();
    private const string ApiToken = "TUNUVS1BQzBtRkF3QzJudmpRTkJUek1wdlRPWFJNeDVYWnY1bUNZVmo4Yz0";
    static async Task Main(string[] args)
    {
        var tickers = await File.ReadAllLinesAsync("ticker.txt");
        var startDate = DateTime.Now.AddMonths(-11).ToString("dd-MM-yyyy");
        var endDate = DateTime.Now.ToString("dd-MM-yyyy");
        var averagePrices = new Dictionary<string, double>();
        var tasks = new List<Task>();
        foreach (var ticker in tickers)
        {
            await Delay(35);
            tasks.Add(Task.Run(async () =>
            {
                var averagePrice = await GetAveragePrice(ticker, startDate, endDate);
                if (averagePrice.HasValue)
                {
                    lock (averagePrices)
                    {
                        averagePrices[ticker] = averagePrice.Value;
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
        foreach (var kvp in averagePrices)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value:F2}");
        }
    }
    private static async Task<double?> GetAveragePrice(string ticker, string startDate, string endDate)
    {
        try
        {
            var url = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from={startDate}&to={endDate}&token={ApiToken}";
            var response = await client.GetStringAsync(url);
            var data = JObject.Parse(response);
            if (data["s"]?.ToString() != "ok")
            {
                Console.WriteLine($"Error retrieving data for {ticker}");
                return null;
            }
            var highs = data["h"]?.ToObject<List<double>>();
            var lows = data["l"]?.ToObject<List<double>>();

            if (highs == null || lows == null || highs.Count != lows.Count)
            {
                Console.WriteLine($"Incomplete data for {ticker}");
                return null;
            }
            double totalPrice = 0;
            for (int i = 0; i < highs.Count; i++)
            {
                totalPrice += (highs[i] + lows[i]) / 2;
            }
            return highs.Count > 0 ? totalPrice / highs.Count : (double?)null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}

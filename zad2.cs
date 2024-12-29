using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http.Json;
public struct Weather
{
    public string Country { get; set; }
    public string Name { get; set; }
    public float Temp { get; set; }
    public string Description { get; set; }
}
class Program : Form
{
    private static readonly HttpClient httpClient = new HttpClient();
    private const string ApiKey = "3637be494f42e767f04edda24ce05113";
    private List<Weather> weatherData = new List<Weather>();
    private ListBox cityListBox;
    private Button loadWeatherButton;
    public Program()
    {
        cityListBox = new ListBox { Dock = DockStyle.Top, Height = 200 };
        loadWeatherButton = new Button { Text = "Загрузить погоду", Dock = DockStyle.Bottom };
        loadWeatherButton.Click += LoadWeatherButton_Click;
        LoadCities();
        Controls.Add(cityListBox);
        Controls.Add(loadWeatherButton);
    }
    private void LoadCities()
    {
        var cities = File.ReadAllLines("city.txt");
        cityListBox.Items.AddRange(cities);
    }
    private async void LoadWeatherButton_Click(object sender, EventArgs e)
    {
        if (cityListBox.SelectedItem != null)
        {
            string selectedCity = cityListBox.SelectedItem.ToString();
            var weather = await GetWeatherData(selectedCity);
            if (weather.HasValue)
            {
                MessageBox.Show($"Погода в {weather.Value.Name}, {weather.Value.Country}: {weather.Value.Temp}°C, {weather.Value.Description}");
            }
            else
            {
                MessageBox.Show("Не удалось получить данные о погоде.");
            }
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите город.");
        }
    }
    private async Task<Weather?> GetWeatherData(string city)
    {
        try
        {
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={ApiKey}&units=metric";
            var response = await httpClient.GetFromJsonAsync<WeatherResponse>(url);
            if (response != null
                && response.Main != null
                && response.Sys != null
                && response.Weather != null
                && response.Name != ""
                && response.Weather.Count > 0)
            {
                return new Weather
                {
                    Country = response.Sys.Country ?? "Неизвестно",
                    Name = response.Name ?? "Неизвестно",
                    Temp = response.Main.Temp,
                    Description = response.Weather[0].Description
                };
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Ошибка запроса: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка: {ex.Message}");
        }
        return null;
    }
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            Application.Run(new Program());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}");
        }
    }
}
public class WeatherResponse
{
    public Main Main { get; set; }
    public Sys Sys { get; set; }
    public List<WeatherDescription> Weather { get; set; }
    public string Name { get; set; }
}
public class Main
{
    public float Temp { get; set; }
}
public class Sys
{
    public string Country { get; set; }
}
public class WeatherDescription
{
    public string Description { get; set; }
}

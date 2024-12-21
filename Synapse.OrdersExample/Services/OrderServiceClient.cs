using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;

public class OrderServiceClient : IOrderServiceClient
{
    private string _retrieveOrdersUrl;
    private string _alertOrdersUrl;
    private string _updateOrdersUrl;

    private readonly HttpClient _httpClient;

    public OrderServiceClient(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _retrieveOrdersUrl = configuration["OrderService:RetrieveOrdersUrl"];
        _alertOrdersUrl = configuration["OrderService:AlertOrdersUrl"];
        _updateOrdersUrl = configuration["OrderService:UpdateOrdersUrl"];
    }

    public async Task<JObject[]> FetchOrdersAsync()
    {
        string ordersApiUrl = _retrieveOrdersUrl;
        var response = await _httpClient.GetAsync(ordersApiUrl);
        response.EnsureSuccessStatusCode();

        var ordersData = await response.Content.ReadAsStringAsync();
        return JArray.Parse(ordersData)?.ToObject<JObject[]>() ?? [];
    }

    public async Task AlertAsync(string orderId, JToken item)
    {
        string alertApiUrl = _alertOrdersUrl;
        var alertData = new
        {
            Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                      $"Delivery Notifications: {item["deliveryNotification"]}"
        };
        var content = new StringContent(JObject.FromObject(alertData).ToString(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(alertApiUrl, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateOrderAsync(JObject order)
    {
        string updateApiUrl = _updateOrdersUrl;
        var content = new StringContent(order.ToString(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(updateApiUrl, content);
        response.EnsureSuccessStatusCode();
    }
}
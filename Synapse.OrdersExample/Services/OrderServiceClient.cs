using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class OrderServiceClient : IOrderServiceClient
{
    private readonly HttpClient _httpClient;

    public OrderServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<JObject[]> FetchOrdersAsync()
    {
        string ordersApiUrl = "https://orders-api.com/orders";
        var response = await _httpClient.GetAsync(ordersApiUrl);
        response.EnsureSuccessStatusCode();

        var ordersData = await response.Content.ReadAsStringAsync();
        return JArray.Parse(ordersData).ToObject<JObject[]>();
    }

    public async Task AlertAsync(string orderId, JToken item)
    {
        string alertApiUrl = "https://alert-api.com/alerts";
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
        string updateApiUrl = "https://update-api.com/update";
        var content = new StringContent(order.ToString(), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(updateApiUrl, content);
        response.EnsureSuccessStatusCode();
    }
}

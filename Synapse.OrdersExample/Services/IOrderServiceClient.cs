using Newtonsoft.Json.Linq;

public interface IOrderServiceClient
{
    Task<JObject[]> FetchOrdersAsync();
    Task AlertAsync(string orderId, JToken item);
    Task UpdateOrderAsync(JObject order);
}

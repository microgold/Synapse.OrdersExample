using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

public class MockOrderServiceClient : IOrderServiceClient
{
    public Task<JObject[]> FetchOrdersAsync()
    {
        // Simulated response for fetching orders
        return Task.FromResult(new[]
        {
            new JObject
            {
                ["OrderId"] = "123",
                ["Items"] = new JArray
                {
                    new JObject
                    {
                        ["Description"] = "Mock Item",
                        ["Status"] = "Delivered",
                        ["deliveryNotification"] = 0
                    }
                }
            }
        });
    }

    public Task AlertAsync(string orderId, JToken item)
    {
        // Simulate alert
        Console.WriteLine($"[Mock] Alert sent for Order {orderId}, Item: {item["Description"]}");
        return Task.CompletedTask;
    }

    public Task UpdateOrderAsync(JObject order)
    {
        // Simulate order update
        Console.WriteLine($"[Mock] Order updated for OrderId: {order["OrderId"]}");
        return Task.CompletedTask;
    }
}

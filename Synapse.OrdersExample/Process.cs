using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Synapse.OrdersExample
{
    public class Process : IProcess
    {
        private readonly IOrderServiceClient _orderServiceClient;
        private readonly ILogger<Process> _logger;

        public Process(IOrderServiceClient orderServiceClient, ILogger<Process> logger)
        {
            _orderServiceClient = orderServiceClient;
            _logger = logger;
        }

        public async Task<int> RunAsync(string[] args)
        {
            _logger.LogInformation("Start of App");

            var medicalEquipmentOrders = await FetchMedicalEquipmentOrders();
            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = await ProcessOrder(order);
                await SendAlertAndUpdateOrder(updatedOrder);
            }

            _logger.LogInformation("Results sent to relevant APIs.");
            return 0;
        }

        public async Task<JObject[]> FetchMedicalEquipmentOrders()
        {
            try
            {
                return await _orderServiceClient.FetchOrdersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch orders from API");
                return Array.Empty<JObject>();
            }
        }

        public async Task<JObject> ProcessOrder(JObject order)
        {
            var items = order["Items"]?.ToObject<JArray>();

            var tasks = items?
                .Where(IsItemDelivered)
                .Select(async item =>
                {
                    await SendAlertMessage(item, order["OrderId"]?.ToString());
                    IncrementDeliveryNotification(item);
                });

            if (tasks != null)
            {
                await Task.WhenAll(tasks);
            }

            return order;
        }

        public bool IsItemDelivered(JToken item)
        {
            return item["Status"]?.ToString()?.Equals("Delivered", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public async Task SendAlertMessage(JToken item, string orderId)
        {
            try
            {
                await _orderServiceClient.AlertAsync(orderId, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send alert for delivered item: {item["Description"]}");
            }
        }

        public void IncrementDeliveryNotification(JToken item)
        {
            item["deliveryNotification"] = item?["deliveryNotification"]?.Value<int>() + 1;
        }

        public async Task SendAlertAndUpdateOrder(JObject order)
        {
            try
            {
                await _orderServiceClient.UpdateOrderAsync(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send updated order for processing: OrderId {order["OrderId"]}");
            }
        }
    }
}
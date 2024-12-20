using Newtonsoft.Json.Linq;

namespace Synapse.OrdersExample
{
    public interface IProcess
    {
        Task<int> RunAsync(string[] args);
        Task<JObject[]> FetchMedicalEquipmentOrders();
        Task<JObject> ProcessOrder(JObject order);
        bool IsItemDelivered(JToken item);
        Task SendAlertMessage(JToken item, string orderId);
        void IncrementDeliveryNotification(JToken item);
        Task SendAlertAndUpdateOrder(JObject order);
    }
}

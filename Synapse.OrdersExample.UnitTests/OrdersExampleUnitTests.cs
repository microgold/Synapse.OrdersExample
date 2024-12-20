using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;

public class ProgramTests
{
    private readonly Mock<IOrderServiceClient> _mockOrderServiceClient;
    private readonly Mock<ILogger<Synapse.OrdersExample.Process>> _mockLogger;
    private readonly Synapse.OrdersExample.Process _process;

    public ProgramTests()
    {
        _mockOrderServiceClient = new Mock<IOrderServiceClient>();
        _mockLogger = new Mock<ILogger<Synapse.OrdersExample.Process>>();

        _process = new Synapse.OrdersExample.Process(_mockOrderServiceClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task FetchMedicalEquipmentOrders_ReturnsOrders_WhenServiceSucceeds()
    {
        // Arrange
        var mockOrders = new[]
        {
            new JObject
            {
                ["OrderId"] = "123",
                ["Items"] = new JArray()
            }
        };
        _mockOrderServiceClient.Setup(client => client.FetchOrdersAsync())
            .ReturnsAsync(mockOrders);

        // Act
        var result = await _process.FetchMedicalEquipmentOrders();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("123", result[0]["OrderId"].ToString());
    }

    [Fact]
    public async Task FetchMedicalEquipmentOrders_ReturnsEmpty_WhenServiceFails()
    {
        // Arrange
        _mockOrderServiceClient.Setup(client => client.FetchOrdersAsync())
            .ThrowsAsync(new System.Exception("Service failure"));

        // Act
        var result = await _process.FetchMedicalEquipmentOrders();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ProcessOrder_AlertsDeliveredItems()
    {
        // Arrange
        var mockOrder = new JObject
        {
            ["OrderId"] = "123",
            ["Items"] = new JArray
            {
                new JObject { ["Status"] = "Delivered", ["Description"] = "Item1", ["deliveryNotification"] = 0 },
                new JObject { ["Status"] = "Pending", ["Description"] = "Item2", ["deliveryNotification"] = 0 }
            }
        };

        // Act
        var result = await _process.ProcessOrder(mockOrder);

        // Assert
        _mockOrderServiceClient.Verify(client =>
            client.AlertAsync("123", It.Is<JToken>(item => item["Description"].ToString() == "Item1")), Times.Once);
        Assert.Equal(0, mockOrder["Items"][0]["deliveryNotification"].Value<int>());
    }

    [Fact]
    public async Task ProcessOrder_DoesNotAlertUndeliveredItems()
    {
        // Arrange
        var mockOrder = new JObject
        {
            ["OrderId"] = "123",
            ["Items"] = new JArray
            {
                new JObject { ["Status"] = "Pending", ["Description"] = "Item1", ["deliveryNotification"] = 0 }
            }
        };

        // Act
        var result = await _process.ProcessOrder(mockOrder);

        // Assert
        _mockOrderServiceClient.Verify(client =>
            client.AlertAsync(It.IsAny<string>(), It.IsAny<JToken>()), Times.Never);
    }

    [Fact]
    public async Task SendAlertMessage_CallsAlertAsync()
    {
        // Arrange
        var mockItem = new JObject { ["Description"] = "Test Item", ["deliveryNotification"] = 0 };
        var orderId = "123";

        // Act
        await _process.SendAlertMessage(mockItem, orderId);

        // Assert
        _mockOrderServiceClient.Verify(client => client.AlertAsync(orderId, mockItem), Times.Once);
    }

    [Fact]
    public async Task SendAlertAndUpdateOrder_CallsUpdateOrderAsync()
    {
        // Arrange
        var mockOrder = new JObject { ["OrderId"] = "123" };

        // Act
        await _process.SendAlertAndUpdateOrder(mockOrder);

        // Assert
        _mockOrderServiceClient.Verify(client => client.UpdateOrderAsync(mockOrder), Times.Once);
    }

    [Fact]
    public void IncrementDeliveryNotification_IncrementsNotificationCount()
    {
        // Arrange
        var mockItem = new JObject { ["deliveryNotification"] = 0 };

        // Act
        _process.IncrementDeliveryNotification(mockItem);

        // Assert
        Assert.Equal(1, mockItem["deliveryNotification"].Value<int>());
    }
}
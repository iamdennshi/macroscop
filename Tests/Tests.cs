using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests
{
    public class ServerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ServerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("2022-02-02T20:02:02", "чет!")]
        [InlineData("2021-11-11T11:11:11", "нечет!")]
        [InlineData("2011-11-11T12:22:22", "равно!")]
        public async Task WebSocketShouldReturnCorrectMessage(string dateTimeString, string expectedMessage)
        {
            var mockTimeService = new Mock<IDateTimeService>(MockBehavior.Strict);
            mockTimeService.Setup(p => p.GetCurrentDateTime()).Returns(DateTime.Parse(dateTimeString));

            var factoryWithMocks = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDateTimeService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddSingleton(mockTimeService.Object);
                });
            });

            var webSocketClient = factoryWithMocks.Server.CreateWebSocketClient();
            using var webSocket = await webSocketClient.ConnectAsync(new Uri("ws://localhost:5000"), CancellationToken.None);

            Assert.Equal(WebSocketState.Open, webSocket.State);

            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var actualMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

            Assert.Equal(expectedMessage, actualMessage);
        }
    }
}

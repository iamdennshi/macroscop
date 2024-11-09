using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Server.Services
{
  public class WebSocketConnectionManager
  {
    private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
      _logger = logger;
    }

    public async Task HandleWebSocketAsync(HttpContext context, WebSocket webSocket)
    {
      var clientIp = context.Connection.RemoteIpAddress?.ToString();
      var clientPort = context.Connection.RemotePort.ToString();
      var clientName = $"{clientIp}:{clientPort}";
      var buffer = new byte[1024 * 4];

      _sockets[clientName] = webSocket;

      _logger.LogInformation($"{clientName} connected ");

      try
      {
        while (webSocket.State == WebSocketState.Open)
        {
          var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

          if (result.MessageType == WebSocketMessageType.Close)
          {
            _sockets.TryRemove(clientName, out _);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocket manager", CancellationToken.None);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError($"{clientName} disconnected: {ex.Message}");

        if (webSocket.State == WebSocketState.Open)
        {
          await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error", CancellationToken.None);
        }

        _sockets.TryRemove(clientName, out _);
      }
    }

    public async Task SendMessageToAllClientsAsync(string message)
    {
      var buffer = Encoding.UTF8.GetBytes(message);
      var tasks = _sockets.Values
          .Where(socket => socket.State == WebSocketState.Open)
          .Select(socket => socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None));

      await Task.WhenAll(tasks);
    }
  }
}

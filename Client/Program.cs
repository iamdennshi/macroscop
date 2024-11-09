using System.Net.WebSockets;
using System.Text;

using var client = new ClientWebSocket();
await client.ConnectAsync(new Uri("ws://localhost:5000/"), CancellationToken.None);

var buffer = new byte[1024];
while (client.State == WebSocketState.Open)
{
  var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
  var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
  Console.WriteLine($"Received message: {message}");
}




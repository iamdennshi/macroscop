using System.Net.WebSockets;
using System.Text;

var serverUrl = Environment.GetEnvironmentVariable("SERVER_URL") ?? "ws://localhost:8080";

bool connected = false;
int retries = 5;

for (int i = 0; i < retries && !connected; i++)
{
  using var client = new ClientWebSocket();
  try
  {
    Console.WriteLine($"Attempting to connect to server {serverUrl}");
    await client.ConnectAsync(new Uri(serverUrl), CancellationToken.None);
    connected = true;

    var buffer = new byte[1024];
    while (client.State == WebSocketState.Open)
    {
      var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
      var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
      Console.WriteLine($"Received message: {message}");
    }
  }
  catch (WebSocketException ex)
  {
    Console.WriteLine($"Connectino faild: {ex.Message}. Retrying in 3 seconds...");
    await Task.Delay(3000);
  }
}

if (!connected)
{
  Console.WriteLine($"Failed to connect to server after {retries} attempts");
}





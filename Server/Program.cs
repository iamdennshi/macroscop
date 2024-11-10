using System.Net;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebSocketConnectionManager>();
builder.Services.AddSingleton<IDateTimeService, DateTimeService>();
builder.Services.AddHostedService<DateTimeCheckerService>();
builder.Services.AddControllers();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseWebSockets();
app.Map("/", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var webSocketConnectionManager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
        await webSocketConnectionManager.HandleWebSocketAsync(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});
app.Run();

public partial class Program { }


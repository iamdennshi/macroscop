namespace Server.Services
{
  public class DateTimeCheckerService : BackgroundService
  {
    private readonly WebSocketConnectionManager _webSocketManager;
    private readonly IDateTimeService _timeService;

    public DateTimeCheckerService(WebSocketConnectionManager webSocketManager, IDateTimeService dateTimeService)
    {
      _webSocketManager = webSocketManager;
      _timeService = dateTimeService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var message = GetDateTimeMessage();
        await _webSocketManager.SendMessageToAllClientsAsync(message);
        await Task.Delay(1000, stoppingToken);
      }
    }

    private string GetDateTimeMessage()
    {
      var date = _timeService.GetCurrentDateTime();
      var digits = date.ToString("yyyyMMddHHmmss").Select(c => c - '0');
      var evenCount = digits.Count(d => d % 2 == 0);
      var oddCount = digits.Count(d => d % 2 != 0);
      var result = evenCount > oddCount ? "чет!" : (evenCount == oddCount ? "равно!" : "нечет!");

      return result;
    }
  }
}

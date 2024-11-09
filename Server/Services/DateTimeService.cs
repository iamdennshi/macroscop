public interface IDateTimeService
{
  DateTime GetCurrentDateTime();
}

public class DateTimeService : IDateTimeService
{
  public DateTime GetCurrentDateTime() => DateTime.Now;
}

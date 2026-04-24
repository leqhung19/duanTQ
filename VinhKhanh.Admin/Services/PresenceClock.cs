namespace VinhKhanh.Admin.Services;

public static class PresenceClock
{
    private static readonly TimeZoneInfo SomeeTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    public static DateTime Now()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SomeeTimeZone);
    }
}

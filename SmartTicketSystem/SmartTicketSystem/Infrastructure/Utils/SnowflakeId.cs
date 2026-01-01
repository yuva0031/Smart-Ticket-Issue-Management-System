namespace SmartTicketSystem.Infrastructure.Utils;
public static class SnowflakeId
{
    private static readonly long Epoch = new DateTime(2023, 1, 1).Ticks;
    private static readonly Random RandomGen = new Random();

    public static long NewId()
    {
        long timestamp = (DateTime.UtcNow.Ticks - Epoch) >> 10;
        long random = RandomGen.Next(0, 1 << 22);
        return (timestamp << 22) | random;
    }
}
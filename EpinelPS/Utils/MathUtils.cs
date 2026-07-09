using Google.Protobuf.WellKnownTypes;
namespace EpinelPS.Utils;

public class MathUtils
{
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            return min;
        else if (value.CompareTo(max) > 0)
            return max;
        else
            return value;
    }
}

public static class DurationExtensions
{
    public static Duration Min(this Duration a, Duration b)
    {
        return a.ToTimeSpan() <= b.ToTimeSpan() ? a : b;
    }

    public static Duration Max(this Duration a, Duration b)
    {
        return a.ToTimeSpan() >= b.ToTimeSpan() ? a : b;
    }

    public static bool LessThan(this Duration a, Duration b)
    {
        return a.ToTimeSpan() < b.ToTimeSpan();
    }

    /// <summary>
    /// 返回两个 Duration 的绝对差值（总是返回正数）
    /// </summary>
    public static Duration TruncatedDuration(this Duration d1, Duration d2)
    {
        if (d1 == null) throw new ArgumentNullException(nameof(d1));
        if (d2 == null) throw new ArgumentNullException(nameof(d2));

        long totalNanos1 = d1.Seconds * 1_000_000_000L + d1.Nanos;
        long totalNanos2 = d2.Seconds * 1_000_000_000L + d2.Nanos;
        long diffNanos = totalNanos1 - totalNanos2;

        Logging.WriteLine($"diffNanos{diffNanos}",LogType.Info);

        return Duration.FromTimeSpan(TimeSpan.FromTicks(diffNanos / 100));
    }
}

public static class ListExtensions
{
    public static void AddUnique<T>(this List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
    }

    public static void AddRangeUnique<T>(this List<T> list, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }
    }

}
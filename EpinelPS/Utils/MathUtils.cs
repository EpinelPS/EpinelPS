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
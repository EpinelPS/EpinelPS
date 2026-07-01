namespace EpinelPS.LobbyServer.Outpost.Dispatch;

public class DispatchHelper
{
    public static T SelectItemByProbability<T>(List<T> items, Func<T, double> probabilitySelector)
    {
        if (!items.Any())
            return default;

        double totalProb = items.Sum(probabilitySelector);

        if (totalProb <= 0)
            return default;

        Random random = new Random();
        double randomValue = random.NextDouble() * totalProb;

        double cumulativeProb = 0;

        foreach (var item in items)
        {
            cumulativeProb += probabilitySelector(item);

            if (randomValue <= cumulativeProb)
            {
                return item;
            }
        }

        return items.Last();
    }
}
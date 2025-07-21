using EpinelPS.Data;

namespace EpinelPS.Utils
{
    public class Rng
    {
        private static readonly Random random = new();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
        }

        public static int RandomId()
        {
            return random.Next();
        } /// <summary>
          /// Picks a random item. weights is a list of numbers which represents probability, table ids represent ID for weight
          /// </summary>
          /// <param name="weights"></param>
          /// <param name="tableIds"></param>
          /// <returns></returns>
          /// <exception cref="Exception"></exception>
        public static RandomItemRecord PickWeightedItem(RandomItemRecord[] records)
        {
            int totalWeight = 0;
            foreach (RandomItemRecord item in records)
                totalWeight += item.ratio;

            int randomNumber = random.Next(0, totalWeight);

            int runningSum = 0;
            for (int i = 0; i < records.Length; i++)
            {
                runningSum += records[i].ratio;
                if (randomNumber < runningSum)
                    return records[i];
            }

            throw new Exception("Weight distribution error.");
        }
    }
}

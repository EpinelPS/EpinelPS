using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/event/boxgacha/execute")]
public class ExecuteEventBoxGacha : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqExecuteEventBoxGacha req = await ReadData<ReqExecuteEventBoxGacha>();
        User user = GetUser();

        // 1. Find EventBoxGachaRecord by EventId
        var boxGacha = GameData.Instance.EventBoxGachaTable.Values.FirstOrDefault(x => x.EventId == req.EventId);
        if (boxGacha == null)
        {
            Logging.WriteLine($"[EventBoxGacha] Unknown event id: {req.EventId}", LogType.Warning);
            await WriteDataAsync(new ResExecuteEventBoxGacha());
            return;
        }

        if (!user.EventBoxGachaData.TryGetValue(req.EventId, out var gachaData))
        {
            gachaData = new UserEventBoxGachaData();
            user.EventBoxGachaData[req.EventId] = gachaData;
        }

        // 2. Safeguard: Check undrawn rewards and box completion BEFORE deducting any tickets
        var drawnOrders = new HashSet<int>(gachaData.RewardOrders);
        var allRewards = GameData.Instance.EventBoxGachaRewardTable.Values
            .Where(r => r.Group == boxGacha.GachaRewardGroup)
            .OrderBy(r => r.Order)
            .ToList();

        if (allRewards.Count == 0)
        {
            Logging.WriteLine($"[EventBoxGacha] Event {req.EventId} has no configured rewards in GachaRewardGroup {boxGacha.GachaRewardGroup}", LogType.Warning);
            await WriteDataAsync(new ResExecuteEventBoxGacha());
            return;
        }

        var availableRewards = allRewards.Where(r => !drawnOrders.Contains(r.Order)).ToList();
        if (availableRewards.Count == 0 || gachaData.GachaCount >= allRewards.Count)
        {
            Logging.WriteLine($"[EventBoxGacha] Event {req.EventId} box is already completed for user {user.ID} ({gachaData.RewardOrders.Count}/{allRewards.Count} rewards drawn)", LogType.Warning);
            await WriteDataAsync(new ResExecuteEventBoxGacha());
            return;
        }

        // 3. Client-server roll count desync check (server is always authoritative)
        if (req.CurrentCount > 0 && req.CurrentCount != gachaData.GachaCount)
        {
            Logging.WriteLine($"[EventBoxGacha] Desync warning for user {user.ID} in event {req.EventId}: client reported count {req.CurrentCount}, server has {gachaData.GachaCount}", LogType.Warning);
        }

        int currentCount = gachaData.GachaCount + 1; // 1-indexed roll number

        // 4. Resolve price from EventBoxGachaPriceTable with safeguards
        var priceRecords = GameData.Instance.EventBoxGachaPriceTable.Values
            .Where(p => p.Group == boxGacha.PriceGroup)
            .OrderBy(p => p.Count)
            .ToList();

        if (priceRecords.Count == 0)
        {
            Logging.WriteLine($"[EventBoxGacha] Event {req.EventId} has no price records in PriceGroup {boxGacha.PriceGroup}", LogType.Warning);
            await WriteDataAsync(new ResExecuteEventBoxGacha());
            return;
        }

        var priceRec = priceRecords.FirstOrDefault(p => p.Count == currentCount);
        if (priceRec == null)
        {
            // Safeguard: If currentCount exceeds defined price count, fallback to the last (max) price record
            // to prevent free rolls or null exceptions
            priceRec = priceRecords.Last();
            Logging.WriteLine($"[EventBoxGacha] Roll count {currentCount} exceeded configured prices. Falling back to max price (Count {priceRec.Count}, Cost {priceRec.ItemCount}).", LogType.Warning);
        }

        // 5. Check and deduct tickets
        int ticketId = priceRec.ItemId > 0 ? priceRec.ItemId : boxGacha.EventItemId;
        var ticketStacks = user.Items.Where(i => i.ItemType == ticketId).ToList();
        int totalTickets = ticketStacks.Sum(i => i.Count);

        // Consolidate into a single primary stack if fragmented across multiple stacks
        if (ticketStacks.Count > 1)
        {
            var primary = ticketStacks[0];
            primary.Count = totalTickets;
            for (int i = 1; i < ticketStacks.Count; i++)
            {
                user.Items.Remove(ticketStacks[i]);
            }
            ticketStacks = [primary];
        }

        DbItemData? ticketItem = ticketStacks.FirstOrDefault();

        // Check if this roll requires tickets (Count 1 is free: ItemCount = 0)
        if (priceRec.ItemType == RewardType.Item && priceRec.ItemCount > 0)
        {
            if (totalTickets < priceRec.ItemCount || ticketItem == null)
            {
                Logging.WriteLine($"[EventBoxGacha] User {user.ID} has insufficient tickets for event {req.EventId}. Needed: {priceRec.ItemCount}, Has: {totalTickets}", LogType.Warning);
                await WriteDataAsync(new ResExecuteEventBoxGacha());
                return;
            }

            ticketItem.Count -= priceRec.ItemCount;
            totalTickets = ticketItem.Count;
            if (ticketItem.Count <= 0)
            {
                user.Items.Remove(ticketItem);
            }
        }
        else
        {
            Logging.WriteLine($"[EventBoxGacha] Roll {currentCount} for user {user.ID} in event {req.EventId} is FREE (Cost: 0).");
        }

        // 6. Calculate probabilities using EventBoxGachaProbTable for currentCount with fallbacks
        var probRecords = GameData.Instance.EventBoxGachaProbTable.Values
            .Where(p => p.Group == boxGacha.ProbGroup && p.Count == currentCount)
            .ToDictionary(p => p.Order, p => p.Rate);

        if (probRecords.Count == 0)
        {
            // Fallback to highest available count if currentCount doesn't have an exact row
            var fallbackCount = GameData.Instance.EventBoxGachaProbTable.Values
                .Where(p => p.Group == boxGacha.ProbGroup)
                .Select(p => p.Count)
                .DefaultIfEmpty(0)
                .Max();

            if (fallbackCount > 0)
            {
                probRecords = GameData.Instance.EventBoxGachaProbTable.Values
                    .Where(p => p.Group == boxGacha.ProbGroup && p.Count == fallbackCount)
                    .ToDictionary(p => p.Order, p => p.Rate);
            }
        }

        int totalWeight = 0;
        foreach (var rew in availableRewards)
        {
            int rate = probRecords.TryGetValue(rew.Order, out var r) ? r : 100;
            totalWeight += rate;
        }

        EventBoxGachaRewardRecord winningReward;
        if (totalWeight <= 0)
        {
            winningReward = availableRewards[Rng.Next(0, availableRewards.Count)];
        }
        else
        {
            int roll = Rng.Next(0, totalWeight);
            int cumulative = 0;
            winningReward = availableRewards.Last();
            foreach (var rew in availableRewards)
            {
                int rate = probRecords.TryGetValue(rew.Order, out var r) ? r : 100;
                cumulative += rate;
                if (roll < cumulative)
                {
                    winningReward = rew;
                    break;
                }
            }
        }

        // 7. Grant winning reward to user
        NetRewardData rewardData = new();
        RewardUtils.AddSingleObject(user, ref rewardData, winningReward.ItemId, winningReward.ItemType, winningReward.ItemCount);

        // 8. Update user box gacha state
        gachaData.RewardOrders.Add(winningReward.Order);
        gachaData.GachaCount = currentCount;
        JsonDb.Save();

        // 9. Build response
        ResExecuteEventBoxGacha response = new()
        {
            Reward = rewardData,
            Ticket = new NetUserItemData
            {
                Tid = ticketId,
                Count = totalTickets,
                Isn = ticketItem?.Isn ?? 0,
                Corporation = ticketItem?.Corp ?? 0,
            },
        };
        response.RewardOrders.AddRange(gachaData.RewardOrders);

        Logging.WriteLine($"[EventBoxGacha] Roll {currentCount} completed for user {user.ID}. Won Order: {winningReward.Order} (Item {winningReward.ItemId} x{winningReward.ItemCount}). Remaining tickets: {totalTickets}");

        await WriteDataAsync(response);
    }
}
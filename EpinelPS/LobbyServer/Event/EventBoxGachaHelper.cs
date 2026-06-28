using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event;

internal static class EventBoxGachaHelper
{
    private static readonly Random Random = new();

    public static ResGetEventBoxGacha Get(User user, int eventId)
    {
        EventBoxGachaState state = GetState(user, eventId);
        ResGetEventBoxGacha response = new()
        {
            GachaCount = state.GachaCount
        };
        response.RewardOrders.AddRange(state.RewardOrders);

        return response;
    }

    public static ResExecuteEventBoxGacha Execute(User user, int eventId, int currentCount)
    {
        EventBoxGachaRecord? box = ResolveBox(eventId);
        EventBoxGachaState state = GetState(user, eventId);
        NetRewardData reward = new() { PassPoint = new() };
        ResExecuteEventBoxGacha response = new()
        {
            Reward = reward
        };

        if (box == null)
        {
            RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, 100);
            response.RewardOrders.AddRange(state.RewardOrders);
            JsonDb.Save();
            Logging.WriteLine($"EventBoxGacha/execute fallback eventId={eventId}: no EventBoxGachaTable row", LogType.Warning);
            return response;
        }

        int drawCount = currentCount <= 0 ? 1 : Math.Clamp(currentCount, 1, 10);
        TryConsumePrice(user, box, drawCount, response);

        List<EventBoxGachaRewardRecord> allRewards = GameData.Instance.EventBoxGachaRewardTable.Values
            .Where(item => item.Group == box.GachaRewardGroup)
            .OrderBy(item => item.Order)
            .ThenBy(item => item.Id)
            .ToList();

        if (allRewards.Count == 0)
        {
            RewardUtils.AddSingleCurrencyObject(user, ref reward, CurrencyType.FreeCash, 100);
            Logging.WriteLine(
                $"EventBoxGacha/execute fallback eventId={eventId} group={box.GachaRewardGroup}: no rewards",
                LogType.Warning);
        }
        else
        {
            for (int i = 0; i < drawCount; i++)
            {
                EventBoxGachaRewardRecord? selected = SelectReward(box, state, allRewards);
                if (selected == null)
                {
                    break;
                }

                state.GachaCount++;
                state.RewardOrders.Add(selected.Order);
                RewardUtils.AddSingleObject(user, ref reward, selected.ItemId, selected.ItemType, selected.ItemCount);
            }
        }

        response.Reward = reward;
        response.RewardOrders.AddRange(state.RewardOrders);
        JsonDb.Save();

        return response;
    }

    private static EventBoxGachaState GetState(User user, int eventId)
    {
        if (!user.EventBoxGachaStates.TryGetValue(eventId, out EventBoxGachaState? state))
        {
            state = new EventBoxGachaState { EventId = eventId };
            user.EventBoxGachaStates[eventId] = state;
        }

        return state;
    }

    private static EventBoxGachaRecord? ResolveBox(int eventId)
    {
        return GameData.Instance.EventBoxGachaTable.Values.FirstOrDefault(item => item.EventId == eventId) ??
            GameData.Instance.EventBoxGachaTable.Values.FirstOrDefault(item => item.Id == eventId);
    }

    private static EventBoxGachaRewardRecord? SelectReward(
        EventBoxGachaRecord box,
        EventBoxGachaState state,
        List<EventBoxGachaRewardRecord> allRewards)
    {
        HashSet<int> obtainedOrders = [.. state.RewardOrders];
        List<EventBoxGachaRewardRecord> remaining = [.. allRewards.Where(item => !obtainedOrders.Contains(item.Order))];

        if (remaining.Count == 0)
        {
            state.RewardOrders.Clear();
            remaining = [.. allRewards];
        }

        int nextCount = state.GachaCount + 1;
        List<EventBoxGachaProbRecord> probRows = GameData.Instance.EventBoxGachaProbTable.Values
            .Where(item => item.Group == box.ProbGroup && item.Count == nextCount && item.Rate > 0)
            .ToList();

        if (probRows.Count == 0)
        {
            probRows = GameData.Instance.EventBoxGachaProbTable.Values
                .Where(item => item.Group == box.ProbGroup && item.Count == 0 && item.Rate > 0)
                .ToList();
        }

        if (probRows.Count == 0)
        {
            probRows = GameData.Instance.EventBoxGachaProbTable.Values
                .Where(item => item.Group == box.ProbGroup && item.Rate > 0)
                .ToList();
        }

        List<EventBoxGachaProbRecord> usableProbRows = [.. probRows.Where(prob => remaining.Any(reward => reward.Order == prob.Order))];
        if (usableProbRows.Count > 0)
        {
            EventBoxGachaProbRecord selectedProb = SelectWeighted(usableProbRows, prob => prob.Rate);
            EventBoxGachaRewardRecord? reward = remaining.FirstOrDefault(item => item.Order == selectedProb.Order);
            if (reward != null)
            {
                return reward;
            }
        }

        return remaining[Random.Next(remaining.Count)];
    }

    private static void TryConsumePrice(User user, EventBoxGachaRecord box, int drawCount, ResExecuteEventBoxGacha response)
    {
        EventBoxGachaPriceRecord? price = GameData.Instance.EventBoxGachaPriceTable.Values
            .Where(item => item.Group == box.PriceGroup && item.Count <= drawCount)
            .OrderByDescending(item => item.Count)
            .FirstOrDefault();

        if (price == null || price.ItemCount <= 0)
        {
            return;
        }

        int totalCost = price.ItemCount * drawCount;

        if (price.ItemType == RewardType.Currency)
        {
            CurrencyType currencyType = (CurrencyType)price.ItemId;
            if (!user.SubtractCurrency(currencyType, totalCost))
            {
                Logging.WriteLine(
                    $"EventBoxGacha price currency missing user={user.ID} currency={currencyType} cost={totalCost}; allowing draw for protocol test",
                    LogType.Warning);
            }
            return;
        }

        if (price.ItemType != RewardType.Item && price.ItemType != box.EventItemType)
        {
            return;
        }

        DbItemData? ticket = user.Items.FirstOrDefault(item => item.ItemType == price.ItemId);
        if (ticket == null || ticket.Count < totalCost)
        {
            Logging.WriteLine(
                $"EventBoxGacha ticket missing user={user.ID} item={price.ItemId} cost={totalCost}; allowing draw for protocol test",
                LogType.Warning);
            return;
        }

        ticket.Count -= totalCost;
        response.Ticket = ToNetItem(ticket);

        if (price.ItemId == box.FreeTicketItemId)
        {
            response.FreeTicket = new NetUserRedeemData()
            {
                Tid = price.ItemId,
                Count = ticket.Count
            };
        }
    }

    private static NetUserItemData ToNetItem(DbItemData item)
    {
        return new NetUserItemData()
        {
            Tid = item.ItemType,
            Csn = item.Csn,
            Count = item.Count,
            Lv = item.Level,
            Exp = item.Exp,
            Position = item.Position,
            Isn = item.Isn,
            Corporation = item.Corp
        };
    }

    private static T SelectWeighted<T>(IReadOnlyList<T> items, Func<T, int> getWeight)
    {
        long totalWeight = items.Sum(item => Math.Max(0, getWeight(item)));
        if (totalWeight <= 0)
        {
            return items[Random.Next(items.Count)];
        }

        long roll = Random.NextInt64(totalWeight);
        long running = 0;
        foreach (T item in items)
        {
            running += Math.Max(0, getWeight(item));
            if (roll < running)
            {
                return item;
            }
        }

        return items[^1];
    }
}

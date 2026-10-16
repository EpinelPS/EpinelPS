using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Event.ChallengeStage;

public static class ChallengeStageHelper
{
    private static readonly ILog log = LogManager.GetLogger(typeof(ChallengeStageHelper));

    /// <summary>
    /// Gets the total number of challenge stages configured for this event dungeon.
    /// </summary>
    public static int GetChallengeStageCount(int eventId)
    {
        var eventStory = GameData.Instance.EventStoryTable.Values.FirstOrDefault(x => x.EventId == eventId);
        if (eventStory != null && GameData.Instance.EventDungeonTable.TryGetValue(eventStory.DungeonId1, out var dungeon))
        {
            var diffs = GameData.Instance.EventDungeonDifficultTable.Values.Where(d => d.Group == dungeon.DifficultGroup);
            int count = 0;
            foreach (var diff in diffs)
            {
                count += GameData.Instance.EventDungeonStageTable.Values.Count(s => s.Group == diff.StageGroup);
            }
            if (count > 0) return count;
        }
        return 5; // Default fallback count
    }

    /// <summary>
    /// Calculates remaining tickets for challenge stages, resetting daily if needed.
    /// </summary>
    public static int GetTicket(User user, int eventId)
    {
        int maxTickets = Math.Max(3, GetChallengeStageCount(eventId));

        if (!user.EventInfo.TryGetValue(eventId, out var eventData))
        {
            eventData = new EventData
            {
                FreeTicket = maxTickets,
                LastDay = user.GetDateDay()
            };
            user.EventInfo.Add(eventId, eventData);
        }

        int dateDay = user.GetDateDay();
        if (dateDay > eventData.LastDay)
        {
            log.Debug($"[ChallengeStage] Resetting daily tickets for event {eventId}: Day {dateDay} > {eventData.LastDay}, tickets={maxTickets}");
            eventData.FreeTicket = maxTickets;
            eventData.LastDay = dateDay;
        }

        int itemTicketCount = 0;
        var eventStory = GameData.Instance.EventStoryTable.Values.FirstOrDefault(x => x.EventId == eventId);
        if (eventStory != null && GameData.Instance.AutoChargeTable.TryGetValue(eventStory.AutoChargeId, out var autoCharge))
        {
            var userItem = user.Items.FirstOrDefault(x => x.ItemType == autoCharge.ItemId);
            if (userItem != null)
            {
                itemTicketCount = userItem.Count;
            }
        }

        int total = eventData.FreeTicket + itemTicketCount;
        log.Debug($"[ChallengeStage] EventId={eventId}, FreeTickets={eventData.FreeTicket}, ItemTickets={itemTicketCount}, Total={total}");
        return total;
    }

    /// <summary>
    /// Subtracts tickets upon battle clear or fast clear.
    /// </summary>
    public static int SubtractTicket(User user, int eventId, int count)
    {
        int maxTickets = Math.Max(3, GetChallengeStageCount(eventId));
        if (!user.EventInfo.TryGetValue(eventId, out var eventData))
        {
            eventData = new EventData
            {
                FreeTicket = maxTickets,
                LastDay = user.GetDateDay()
            };
            user.EventInfo.Add(eventId, eventData);
        }

        if (eventData.FreeTicket >= count)
        {
            eventData.FreeTicket -= count;
        }
        else
        {
            int remainingToSubtract = count - eventData.FreeTicket;
            eventData.FreeTicket = 0;

            var eventStory = GameData.Instance.EventStoryTable.Values.FirstOrDefault(x => x.EventId == eventId);
            if (eventStory != null && GameData.Instance.AutoChargeTable.TryGetValue(eventStory.AutoChargeId, out var autoCharge))
            {
                var userItem = user.Items.FirstOrDefault(x => x.ItemType == autoCharge.ItemId);
                if (userItem != null)
                {
                    user.RemoveItemBySerialNumber(userItem.Isn, remainingToSubtract);
                }
            }
        }

        return GetTicket(user, eventId);
    }

    /// <summary>
    /// Clears stage rewards, awarding first-clear rewards (if first clear) and regular clear rewards.
    /// </summary>
    public static void ClearStage(User user, int stageId, ref NetRewardData reward, ref NetRewardData firstClearReward, bool isFirstClear, int battleResult, int clearCount)
    {
        if (battleResult != 1) return;
        if (clearCount < 1) clearCount = 1;

        if (!GameData.Instance.EventDungeonSpotBattleTable.TryGetValue(stageId, out var spotBattle))
        {
            log.Warn($"[ChallengeStage] StageId {stageId} not found in EventDungeonSpotBattleTable");
            return;
        }

        // 1. Regular clear reward
        if (spotBattle.ClearRewardId > 0)
        {
            ReceivedReward(user, ref reward, spotBattle.ClearRewardId, clearCount);
        }

        // 2. First clear reward (only on first clear)
        if (isFirstClear && spotBattle.FirstClearRewardId > 0)
        {
            ReceivedReward(user, ref firstClearReward, spotBattle.FirstClearRewardId, 1);
        }
    }

    private static void ReceivedReward(User user, ref NetRewardData reward, int rewardId, int clearCount)
    {
        RewardRecord? rewardData = GameData.Instance.GetRewardTableEntry(rewardId);
        if (rewardData == null)
        {
            log.Error($"[ChallengeStage] Unknown reward Id {rewardId}");
            return;
        }

        foreach (var item in rewardData.Rewards)
        {
            if (item == null || item.RewardType == RewardType.None) continue;
            RewardUtils.AddSingleObject(user, ref reward, item.RewardId, item.RewardType, item.RewardValue * clearCount);
        }
    }
}

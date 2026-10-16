using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using log4net;

namespace EpinelPS.LobbyServer.Event.Mission;

[GameRequest("/event/dailymission/obtainreward")]
public class ObtainDailyMissionReward : LobbyMessage
{
    private static readonly ILog log = LogManager.GetLogger(typeof(ObtainDailyMissionReward));

    protected override async Task HandleAsync()
    {
        // ReqObtainDailyEventReward Fields:
        //   int EventId
        //   RepeatedField<int> DailyEventId
        var req = await ReadData<ReqObtainDailyEventReward>();
        User user = GetUser();

        ResObtainDailyEventReward response = new();
        var reward = new NetRewardData();

        try
        {
            if (req.EventId == 0 || req.DailyEventId.Count == 0)
            {
                response.Reward = reward;
                await WriteDataAsync(response);
                return;
            }

            // Ensure user has EventMissionData entry for this event
            if (!user.EventMissionInfo.TryGetValue(req.EventId, out var userEvent))
            {
                userEvent = new EventMissionData();
                user.EventMissionInfo[req.EventId] = userEvent;
            }

            userEvent.MissionIdList ??= [];

            bool hasChanges = false;

            foreach (var dailyEventId in req.DailyEventId)
            {
                // Prevent duplicate claims
                if (userEvent.MissionIdList.Contains(dailyEventId))
                {
                    log.Debug($"Daily event mission {dailyEventId} already claimed for user {user.ID}");
                    continue;
                }

                if (!GameData.Instance.DailyEventTable.TryGetValue(dailyEventId, out var dailyRecord))
                {
                    Logging.Warn($"Daily event record {dailyEventId} not found in DailyEventTable");
                    continue;
                }

                // Collect reward if specified
                if (dailyRecord.RewardId > 0)
                {
                    var rewardRecord = GameData.Instance.GetRewardTableEntry(dailyRecord.RewardId);
                    if (rewardRecord != null && rewardRecord.Rewards != null)
                    {
                        foreach (var item in rewardRecord.Rewards)
                        {
                            RewardUtils.AddSingleObject(user, ref reward, item.RewardId, item.RewardType, item.RewardValue);
                        }
                    }
                }

                // Advance triggers:
                // 1. If not the Day main box itself, advance that day's mission clear count:
                //    Trigger.DailyEventClear (ConditionId = PhaseGroupId, Value = 1)
                //    When 5 sub-missions are claimed, the Day main mission (CondVal = 5) unlocks.
                // 2. Advance the overall milestone counter for the event:
                //    Trigger.EventPoint (ConditionId = EventId, Value = PointValue > 0 ? PointValue : 1)
                //    Day by Day Privaty unlocks at 45 points; Alice's Diary Vouchers unlocks at 70 points.
                if (!dailyRecord.IsMain)
                {
                    if (dailyRecord.EventPhaseGroupId > 0)
                    {
                        user.AddTrigger(Trigger.DailyEventClear, 1, dailyRecord.EventPhaseGroupId);
                    }

                    int pointValue = dailyRecord.PointValue > 0 ? dailyRecord.PointValue : 1;
                    user.AddTrigger(Trigger.EventPoint, pointValue, req.EventId);
                }

                userEvent.MissionIdList.Add(dailyEventId);
                hasChanges = true;

                // Check for final milestone completion (e.g. 200011501 for Day by Day)
                if (dailyRecord.EventPhaseType == EventPhaseType.Final)
                {
                    userEvent.AllClear = true;
                }
            }

            if (hasChanges)
            {
                userEvent.LastDate = DateTime.UtcNow.Ticks;
                JsonDb.Save();
            }
        }
        catch (Exception ex)
        {
            Logging.Warn($"ObtainDailyMissionReward failed for user {user.ID}, event {req.EventId}: {ex.Message}");
        }

        response.Reward = reward;
        await WriteDataAsync(response);
    }
}

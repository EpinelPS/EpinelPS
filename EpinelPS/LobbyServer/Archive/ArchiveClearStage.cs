using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive;

[GameRequest("/archive/storydungeon/clearstage")]
public class ClearArchiveStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArchiveStage req = await ReadData<ReqClearArchiveStage>(); // has fields EventId, StageId, BattleResult
        int evid = req.EventId;
        int stgid = req.StageId;
        int result = req.BattleResult;
        User user = GetUser();

        // Check if the EventInfo exists for the given EventId
        if (!user.EventInfo.TryGetValue(evid, out EventData? eventData))
        {
            eventData = new EventData();
            user.EventInfo[evid] = eventData;
        }

        ResClearArchiveStage response = new();

        // Update the EventData if BattleResult is 1
        if (result == 1)
        {
            bool isFirstClear = !eventData.ClearedStages.Contains(stgid);
            if (isFirstClear)
            {
                eventData.ClearedStages.Add(stgid);
            }

            // Update the LastStage in EventData
            eventData.LastStage = stgid;

            // Resolve difficulty ID from ArchiveEventDungeonStageTable and ArchiveEventDungeonDifficultTable
            if (GameData.Instance.archiveEventDungeonStageRecords.TryGetValue(stgid, out var stageRec))
            {
                var diff = GameData.Instance.ArchiveEventDungeonDifficultRecords.Values
                    .FirstOrDefault(d => d.StageGroup == stageRec.Group);
                if (diff != null)
                {
                    eventData.Diff = diff.Id;
                }
            }

            // Grant first clear rewards if applicable
            if (isFirstClear && GameData.Instance.ArchiveEventDungeonSpotBattleRecords.TryGetValue(stgid, out var spotBattle))
            {
                if (spotBattle != null && spotBattle.FirstClearRewardId > 0)
                {
                    response.Reward = RewardUtils.RegisterRewardsForUser(user, spotBattle.FirstClearRewardId);
                }
            }

            JsonDb.Save();
        }

        // Send the response back to the client
        await WriteDataAsync(response);
    }
}

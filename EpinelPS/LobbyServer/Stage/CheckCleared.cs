using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Stage;

[GameRequest("/stage/checkclear")]
public class CheckCleared : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckStageClear req = await ReadData<ReqCheckStageClear>();

        ResCheckStageClear response = new();
        User user = GetUser();

        HashSet<int> clearedStageIds = [];

        foreach (KeyValuePair<string, FieldInfoNew> fields in user.FieldInfoNew)
        {
            foreach (int stageId in fields.Value.CompletedStages)
            {
                if (req.StageIds.Contains(stageId))
                    clearedStageIds.Add(stageId);
            }
        }

        // Also check if any requested stages belong to completed subquests across all trigger types
        foreach (KeyValuePair<int, bool> subQuest in user.SubQuestData)
        {
            if (!subQuest.Value) continue;

            if (GameData.Instance.Subquests.TryGetValue(subQuest.Key, out SubQuestRecord? subQuestRecord))
            {
                int condId = subQuestRecord.ClearConditionId;
                if (condId <= 0) continue;

                foreach (int stageId in req.StageIds)
                {
                    if (clearedStageIds.Contains(stageId)) continue;

                    if (stageId == condId)
                    {
                        clearedStageIds.Add(stageId);
                        continue;
                    }

                    CampaignStageRecord? stageData = GameData.Instance.GetStageData(stageId);
                    if (stageData != null && stageData.GroupId != 0 && stageData.GroupId == condId)
                    {
                        clearedStageIds.Add(stageId);
                    }
                }
            }
        }

        response.ClearedStageIds.AddRange(clearedStageIds);

        await WriteDataAsync(response);
    }
}

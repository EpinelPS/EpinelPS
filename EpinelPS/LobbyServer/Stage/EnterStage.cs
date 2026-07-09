using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Stage;

[GameRequest("/stage/enterstage")]
public class EnterStage : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterStage req = await ReadData<ReqEnterStage>();
        User user = GetUser();

        ResEnterStage response = new();

        CampaignStageRecord clearedStage = GameData.Instance.GetStageData(req.StageId) ?? throw new Exception("cleared stage cannot be null");
        string map = GameData.Instance.GetMapIdFromChapter(clearedStage.ChapterId, clearedStage.ChapterMod);

        if (clearedStage.StageCategory == StageCategory.Boss)
        {
            // When entering a boss stage, unlock boss information in campaign
            var field = user.FieldInfo.FirstOrDefault(f => f.MapName == map);

            if (field == null)
            {
                field = new FieldInfoNew
                {
                    MapName = map
                };
                user.FieldInfo.Add(field);
            }

            field.BossEntered = true;
        }

        user.AddTrigger(Trigger.CampaignStart, 1, req.StageId);

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}

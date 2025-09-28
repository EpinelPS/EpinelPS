using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/enterstage")]
    public class EnterStage : LobbyMsgHandler
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
                if (!user.FieldInfoNew.ContainsKey(map))
                    user.FieldInfoNew.Add(map, new FieldInfoNew());

                if (user.FieldInfoNew.TryGetValue(map, out FieldInfoNew? info))
                    info.BossEntered = true;
            }

            user.AddTrigger(Trigger.CampaignStart, 1, req.StageId);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

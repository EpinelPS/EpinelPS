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
            var req = await ReadData<ReqEnterStage>();
            var user = GetUser();

            var response = new ResEnterStage();

            var clearedStage = GameData.Instance.GetStageData(req.StageId) ?? throw new Exception("cleared stage cannot be null");
            var map = GameData.Instance.GetMapIdFromChapter(clearedStage.chapter_id, clearedStage.mod);

            if (clearedStage.stage_category ==  StageCategory.Boss)
            {
                // When entering a boss stage, unlock boss information in campaign
                if (!user.FieldInfoNew.ContainsKey(map))
                    user.FieldInfoNew.Add(map, new FieldInfoNew());

                if (user.FieldInfoNew.TryGetValue(map, out FieldInfoNew? info))
                    info.BossEntered = true;
            }

            user.AddTrigger(TriggerType.CampaignStart, 1);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

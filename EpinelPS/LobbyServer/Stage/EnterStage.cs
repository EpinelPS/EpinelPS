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

            if (clearedStage.stage_category == "Boss")
            {
                // When entering a boss stage, unlock boss information in campaign
                var key = (clearedStage.chapter_id - 1) + "_" + clearedStage.chapter_mod;
                if (!user.FieldInfoNew.ContainsKey(key))
                    user.FieldInfoNew.Add(key, new FieldInfoNew());

                if (user.FieldInfoNew.TryGetValue(key, out FieldInfoNew? info))
                    info.BossEntered = true;
            }

            user.AddTrigger(TriggerType.CampaignStart, 1);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

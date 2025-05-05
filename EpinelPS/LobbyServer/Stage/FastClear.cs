using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/fastclearstage")]
    public class FastClear : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearCampaignStage>();

            var user = GetUser();

            Console.WriteLine($"Stage " + req.CampaignStageId + " completed using quick battle");

            var rsp = ClearStage.CompleteStage(user, req.CampaignStageId);

            var response = new ResFastClearCampaignStage()
            {
                OutpostBattle = rsp.OutpostBattle,
                OutpostBattleLevelReward = rsp.OutpostBattleLevelReward,
                OutpostTimeRewardBuff = rsp.OutpostTimeRewardBuff,
                StageClearReward = rsp.StageClearReward,
                UserLevelUpReward = rsp.UserLevelUpReward
            };
            
            await WriteDataAsync(response);
        }
    }

}

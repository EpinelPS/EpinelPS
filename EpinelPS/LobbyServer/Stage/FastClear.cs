using EpinelPS.Utils;
using Google.Protobuf;

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
                StageClearReward = rsp.StageClearReward,
                UserLevelUpReward = rsp.UserLevelUpReward
            };

            response.OutpostTimeRewardBuff.TimeRewardBuffs.AddRange(rsp.OutpostTimeRewardBuff.TimeRewardBuffs);
            
            await WriteDataAsync(response);
        }
    }

}

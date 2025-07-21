using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/fastclearstage")]
    public class FastClear : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearCampaignStage req = await ReadData<ReqFastClearCampaignStage>();

            User user = GetUser();

            Console.WriteLine($"Stage " + req.CampaignStageId + " completed using quick battle");

            ResClearStage rsp = ClearStage.CompleteStage(user, req.CampaignStageId);

            ResFastClearCampaignStage response = new()
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

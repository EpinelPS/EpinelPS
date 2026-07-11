using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.WellKnownTypes;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/completemission")]
public class CompleteMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteNKSV2Mission req = await ReadData<ReqCompleteNKSV2Mission>();
        User user = GetUser();
        ResCompleteNKSV2Mission response = new();


        NetRewardData ret = new();
        List<int> rewardlist = [];

        if (user.Nksv2Datas.TryGetValue(req.NKsId, out var nksv2Data))
        {
            if (req.NKsMissionSeqIdList.Count > 0)
            {
                foreach (var item in req.NKsMissionSeqIdList)
                {
                    var newdata = nksv2Data.MissionProgressData.Values.FirstOrDefault(x => x.Seq == item);
                    if (newdata != null)
                    {
                        newdata.ReceivedAt = DateTime.UtcNow.ToTimestamp();

                        EventNKSMissionRecord? reward = GameData.Instance.EventNKSMissionTable.Values
                            .Where(x => x.Id == newdata.NKsMissionId).FirstOrDefault();
                        if (reward.RewardType == EventNKSMissionRewardTypeData.Item)
                        {
                            rewardlist.Add(reward.RewardValue);
                        }
                    }
                }
            }
            nksv2Data.ProgressJson = req.ProgressJsonAfterComplete;

            if (rewardlist.Count > 0) { ret = RewardUtils.RegisterRewardsForUserDou(user, rewardlist); }

            var missprodata = MiniGameHelper
                .ToProtoDict<int, NetMiniGameNKSV2MissionProgress, MiniGameNKSV2MissionProgress>(nksv2Data.MissionProgressData);

            response.MissionProgressList.AddRange(missprodata.Values);
            response.Error = NKSV2MissionExpiredError.Succeed;
            response.BanResult = MiniGameBanResult.Success;
            response.Reward = ret;
        }



        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
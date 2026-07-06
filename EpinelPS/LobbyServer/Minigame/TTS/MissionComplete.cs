using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using System.Reflection;

namespace EpinelPS.LobbyServer.Minigame.TTS;

[GameRequest("/MiniGame/TTS/Mission/Complete")]
public class MissionComplete : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteMiniGameTtsMission req = await ReadData<ReqCompleteMiniGameTtsMission>();
        User user = GetUser();
        ResCompleteMiniGameTtsMission response = new();

        NetRewardData ret = new();
        if (user.TTSGameData.TryGetValue(req.EventTtsManagerTableId, out var ttsData))
        {
            List<int> rewardIds = [];

            foreach (int item in req.EventTtsMissionTableIdList)
            {
                EventTTSMissionRecord_Raw? mission = GameData.Instance.EventTTSMissionTable.Values
                .Where(m => m.Id == item).FirstOrDefault();

                user.AddUnique(ttsData.MissionCompleteList, item);

                if (ttsData.MissionData.TryGetValue(item, out var miss))
                {
                    miss.IsReceived = true;
                }
                rewardIds.Add(mission.RewardId);
               
            }
            ret = RewardUtils.RegisterRewardsForUserDou(user, rewardIds);
            response.RewardData = ret;
            response.Result = MiniGameTtsMissionCompleteResult.Success;
            response.UpdatedMissionDataList.AddRange(ttsData.MissionData.Values.ToList());
            
        }

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}

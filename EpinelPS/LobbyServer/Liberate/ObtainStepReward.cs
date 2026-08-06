using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/obtainstepreward")]
public class ObtainStepReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainLiberateStepReward req = await ReadData<ReqObtainLiberateStepReward>();
        User user = GetUser();
        ResObtainLiberateStepReward response = new();

        NetRewardData ret = new();
        if (user.LiberateDatas.TryGetValue(user.CurCharacterIdId, out var liberateData))
        {
            LiberateCharacterRecord? liberatechar = GameData.Instance.LiberateCharacterTable.Values
            .Where(x => x.Id == user.CurCharacterIdId).FirstOrDefault();

            LiberateMissionGroupRecord? group = GameData.Instance.LiberateMissionGroupTable.Values
                .Where(x => x.MissionStepGroup == liberatechar.MissionStepGroup && x.MissionStep == liberateData.Step)
                .FirstOrDefault();

            ret = RewardUtils.RegisterRewardsForUser(user, group.RewardId);

            if (liberateData.Step < 5)
            {
                liberateData.Step += 1;
                liberateData.ProgressPoint = 0;
            }
            else
            {
                liberateData.IsCompleted = true;
            }
            response.Data = liberateData;
            response.Error = LiberateDataExpiredError.Success;
            response.Reward = ret;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
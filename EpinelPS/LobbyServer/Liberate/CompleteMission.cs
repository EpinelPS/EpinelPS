using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate;

[GameRequest("/liberate/completemission")]
public class CompleteMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCompleteLiberateMission req = await ReadData<ReqCompleteLiberateMission>();
        User user = GetUser();
        ResCompleteLiberateMission response = new();

        LiberateMissionRecord? mission = GameData.Instance.LiberateMissionTable.Values
            .Where(x => x.Id == req.MissionId).FirstOrDefault();

        if (user.LiberateDatas.TryGetValue(user.CurCharacterIdId, out var liberateData))
        {
            NetLiberateMissionData? missionState = liberateData.MissionData.FirstOrDefault(x => x.Id == req.MissionId);
            if (missionState.CreatedAt.IsOlderThan24Hours())
            {
                missionState.MissionState = LiberateMissionState.Closed;
                response.Data = liberateData;
                response.Error = LiberateDataExpiredError.Expired;
            }
            else
            {
                missionState.MissionState = LiberateMissionState.Rewarded;
                liberateData.ProgressPoint += mission.MissionPointValue;
                liberateData.RewardedCount += 1;
                response.Data = liberateData;
                response.Error = LiberateDataExpiredError.Success;
            }
        }

        JsonDb.Save();
        // TODO
        await WriteDataAsync(response);
    }
}
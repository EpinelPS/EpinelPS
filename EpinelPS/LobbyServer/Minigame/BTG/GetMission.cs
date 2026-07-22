using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/getmission")]
public class GetMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBtgMissionData req = await ReadData<ReqGetArcadeBtgMissionData>();
        User user = GetUser();
        ResGetArcadeBtgMissionData response = new();

        if (user.BtgData.TryGetValue(req.BtgId, out var btgData))
        {
            var missionlist = MiniGameHelper.ToProtoDict<int, NetMiniGameBtgMissionData, MiniGameBtgMissionData>(btgData.MissionDatas);
            response.AchievementMissionDataList.AddRange(missionlist.Values);
        }

        // TODO
        await WriteDataAsync(response);
    }
}
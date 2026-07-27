using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/getmission")]
public class GetMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBubbleMarchMissionData req = await ReadData<ReqGetArcadeBubbleMarchMissionData>();
        User user = GetUser();
        ResGetArcadeBubbleMarchMissionData response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
              .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            response.AchievementMissionDataList.AddRange(MiniGameHelper
                .ToProtoDict<int, NetMiniGameBubbleMarchMissionData, MiniGameBubbleMarchMissionData>(marchData.AchievementMissionDataList).Values);
        }
        // TODO
        
        await WriteDataAsync(response);
    }
}
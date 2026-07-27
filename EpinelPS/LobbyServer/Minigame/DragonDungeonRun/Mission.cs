namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/mission")]
public class Mission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDragonDungeonRunMissionProgress req = await ReadData<ReqGetArcadeDragonDungeonRunMissionProgress>();
        User user = GetUser();
        ResGetArcadeDragonDungeonRunMissionProgress response = new();

        var missiondata = MiniGameHelper
            .ToProtoDict<int, NetDragonDungeonRunMissionProgress, DragonDungeonRunMissionProgress>(user.DDRDatas.MissionDatas);

        response.MissionProgressList.AddRange(missiondata.Values);
        response.RewardedMissionIdList.AddRange(user.DDRDatas.RewardedMissionIdList);
        // TODO
        await WriteDataAsync(response);
    }
}
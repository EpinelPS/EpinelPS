using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/stage/enter")]
public class StageEnter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArcadeDragonDungeonRunStage req = await ReadData<ReqEnterArcadeDragonDungeonRunStage>();
        User user = GetUser();
        ResEnterArcadeDragonDungeonRunStage response = new();

        user.DDRDatas.LastPlayCharacter = req.CharacterId;
        user.DDRDatas.Phase = req.Phase;

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/unlock-state/get")]
public class UnlockStateGet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDragonDungeonRunUnlockState req = await ReadData<ReqGetArcadeDragonDungeonRunUnlockState>();
        User user = GetUser();
        ResGetArcadeDragonDungeonRunUnlockState response = new()
        {
            IsPhase1Unlocked = user.DDRDatas.IsPhase1Unlocked,
            IsPhase2Unlocked = user.DDRDatas.IsPhase2Unlocked
        };
        response.NewCharacterIdList.AddRange(user.DDRDatas.NewCharacters);
        response.LastPlayedCharacter = user.DDRDatas.LastPlayCharacter;
        response.OwnedCharacterIdList.AddRange(user.DDRDatas.Characters);

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
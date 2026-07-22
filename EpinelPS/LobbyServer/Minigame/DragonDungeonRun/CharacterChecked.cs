namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/character/checked")]
public class CharacterChecked : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetCheckedArcadeDragonDungeonRunCharacter req = await ReadData<ReqSetCheckedArcadeDragonDungeonRunCharacter>();
        User user = GetUser();
        ResSetCheckedArcadeDragonDungeonRunCharacter response = new();

        // TODO
        await WriteDataAsync(response);
    }
}
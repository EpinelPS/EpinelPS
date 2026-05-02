using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX;

[GameRequest("/event/minigame/azx/set/tutorial/confirmed")]
public class SetAzxTutorialConfirmed : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        await ReadData<ReqSetMiniGameAzxTutorialConfirmed>();
        User user = GetUser();

        ResSetMiniGameAzxTutorialConfirmed response = new();

        AzxHelper.SetTutorialConfirmed(user, 1);

        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
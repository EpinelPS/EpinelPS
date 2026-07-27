using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/tutorial/set")]
public class TutorialSet : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetArcadeBtgTutorialState req = await ReadData<ReqSetArcadeBtgTutorialState>();
        User user = GetUser();
        ResSetArcadeBtgTutorialState response = new();

        if (user.BtgData.TryGetValue(req.BtgId, out var btgData))
        {
            btgData.Data.TutorialState = req.TutorialState;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
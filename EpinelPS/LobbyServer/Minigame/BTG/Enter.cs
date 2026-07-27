using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/enter")]
public class Enter : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqEnterArcadeBtg req = await ReadData<ReqEnterArcadeBtg>();
        User user = GetUser();
        ResEnterArcadeBtg response = new();

        // TODO        
        await WriteDataAsync(response);
    }
}
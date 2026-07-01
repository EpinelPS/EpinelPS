using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Dispatch;

[GameRequest("/outpost/dispatch/deleteselect")]
public class DeleteDispatch : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqDeleteDispatchSelection req = await ReadData<ReqDeleteDispatchSelection>();

        ResDeleteDispatchSelection response = new();

        Logging.WriteLine($"List {req.SelectableDispatchList}",LogType.Info);
        
        

        // TODO
        await WriteDataAsync(response);
    }
}

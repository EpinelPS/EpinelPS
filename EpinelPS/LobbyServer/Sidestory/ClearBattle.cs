using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/cut/clearbattle")]
    public class ClearBattle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearSideStoryCutForBattle req = await ReadData<ReqClearSideStoryCutForBattle>();
            User user = GetUser();

            ResClearSideStoryCutForBattle response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

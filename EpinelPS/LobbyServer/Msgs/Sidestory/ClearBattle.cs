using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Sidestory
{
    [PacketPath("/sidestory/cut/clearbattle")]
    public class ClearBattle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearSideStoryCutForBattle>();
            var user = GetUser();

            var response = new ResClearSideStoryCutForBattle();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

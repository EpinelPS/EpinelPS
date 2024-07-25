using nksrv.Net;
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Sidestory
{
    [PacketPath("/sidestory/cut/enterbattle")]
    public class EnterBattle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterSideStoryCutForBattle>();
            var user = GetUser();

            var response = new ResEnterSideStoryCutForBattle();

            await WriteDataAsync(response);
        }
    }
}

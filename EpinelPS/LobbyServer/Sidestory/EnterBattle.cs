using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
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

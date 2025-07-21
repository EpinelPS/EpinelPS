using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/cut/enterbattle")]
    public class EnterBattle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterSideStoryCutForBattle req = await ReadData<ReqEnterSideStoryCutForBattle>();
            User user = GetUser();

            ResEnterSideStoryCutForBattle response = new();

            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Tower
{
    [PacketPath("/tower/entertower")]
    public class EnterTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterTower>();

            var response = new ResEnterTower();

            await WriteDataAsync(response);
        }
    }
}

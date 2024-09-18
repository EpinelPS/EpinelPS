using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Tower
{
    [PacketPath("/tower/fastcleartower")]
    public class FastClearTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFastClearTower>();

            var response = new ResFastClearTower();

            await WriteDataAsync(response);
        }
    }
}

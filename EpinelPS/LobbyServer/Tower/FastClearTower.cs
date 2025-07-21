using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Tower
{
    [PacketPath("/tower/fastcleartower")]
    public class FastClearTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearTower req = await ReadData<ReqFastClearTower>();

            ResFastClearTower response = new();

            await WriteDataAsync(response);
        }
    }
}

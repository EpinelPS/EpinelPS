using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Tower
{
    [PacketPath("/tower/entertower")]
    public class EnterTower : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqEnterTower req = await ReadData<ReqEnterTower>();
            User user = GetUser();

            ResEnterTower response = new();


            user.AddTrigger(Trigger.TowerAllStart, 1);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

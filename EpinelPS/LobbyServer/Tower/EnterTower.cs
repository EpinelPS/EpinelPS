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
            var req = await ReadData<ReqEnterTower>();
            var user = GetUser();

            var response = new ResEnterTower();


            user.AddTrigger(TriggerType.TowerAllStart, 1);
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

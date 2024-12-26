using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/obtain/daily")]
    public class ObtainDaily : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqObtainDailyMissionReward>();

            var response = new ResObtainDailyMissionReward();

            // TODO
            await WriteDataAsync(response);
        }
    }
}

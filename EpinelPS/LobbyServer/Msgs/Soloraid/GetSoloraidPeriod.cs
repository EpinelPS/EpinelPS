using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Soloraid
{
    [PacketPath("/soloraid/getperiod")]
    public class GetSoloraidPeriod : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetSoloRaidPeriod>();

            var response = new ResGetSoloRaidPeriod();
            response.Period = new NetSoloRaidPeriodData
            {

            };
            // TODO
            await WriteDataAsync(response);
        }
    }
}

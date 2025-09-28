using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.SoloraId
{
    [PacketPath("/soloraid/getperiod")]
    public class GetSoloraidPeriod : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetSoloRaidPeriod req = await ReadData<ReqGetSoloRaidPeriod>();

            ResGetSoloRaidPeriod response = new()
            {
                Period = new NetSoloRaidPeriodData
                {

                }
            };
            // TODO
            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.SoloraId
{
    [PacketPath("/soloraId/getperiod")]
    public class GetSoloraIdPeriod : LobbyMsgHandler
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

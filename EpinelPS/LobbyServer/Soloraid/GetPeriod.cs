using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid
{
    [PacketPath("/soloraid/getperiod")]
    public class GetPeriod : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetSoloRaidPeriod>();

            ResGetSoloRaidPeriod response = new()
            {
                Period = SoloRaidHelper.GetSoloRaidPeriod()
            };
            // TODO
            await WriteDataAsync(response);
        }
    }
}

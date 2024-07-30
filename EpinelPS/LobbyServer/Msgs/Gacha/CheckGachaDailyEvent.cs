using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Gacha
{
    [PacketPath("/gacha/event/check")]
    public class CheckGachaDailyEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCheckDailyFreeGacha>();

            var response = new ResCheckDailyFreeGacha();

            // TODO implement
            response.FreeCount = 0;
            response.EventData = new NetEventData() { Id = 1 };

            await WriteDataAsync(response);
        }
    }
}

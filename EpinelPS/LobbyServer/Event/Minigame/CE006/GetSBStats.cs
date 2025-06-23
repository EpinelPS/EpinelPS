using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.CE006
{
    [PacketPath("/event/minigame/stellar-blade/statistics/get")]
    public class GetSBStats : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetStellarBladeStatistics>();
            var user = GetUser();

            var response = new ResGetStellarBladeStatistics();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}

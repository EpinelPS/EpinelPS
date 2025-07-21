using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.CE006
{
    [PacketPath("/event/minigame/stellar-blade/statistics/get")]
    public class GetSBStats : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetStellarBladeStatistics req = await ReadData<ReqGetStellarBladeStatistics>();
            User user = GetUser();

            ResGetStellarBladeStatistics response = new();

            // TODO implement

            await WriteDataAsync(response);
        }
    }
}

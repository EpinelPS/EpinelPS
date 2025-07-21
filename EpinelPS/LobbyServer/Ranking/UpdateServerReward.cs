using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Ranking
{
    [PacketPath("/ranking/updateserverreward")]
    public class UpdateServerReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqUpdateRankingServerReward req = await ReadData<ReqUpdateRankingServerReward>();
            ResUpdateRankingServerReward response = new();


            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Ranking
{
    [PacketPath("/ranking/alltoprank")]
    public class GetTopRanks : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetAllTopRank>();

            var response = new ResGetAllTopRank();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

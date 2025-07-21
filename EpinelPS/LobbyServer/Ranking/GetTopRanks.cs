using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Ranking
{
    [PacketPath("/ranking/alltoprank")]
    public class GetTopRanks : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetAllTopRank req = await ReadData<ReqGetAllTopRank>();

            ResGetAllTopRank response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

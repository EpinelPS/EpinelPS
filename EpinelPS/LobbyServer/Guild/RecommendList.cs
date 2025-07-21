using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Guild
{
    [PacketPath("/guild/recommendlist")]
    public class GetRecommendList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqRecommendGuildList req = await ReadData<ReqRecommendGuildList>();
            ResRecommendGuildList response = new();


            await WriteDataAsync(response);
        }
    }
}

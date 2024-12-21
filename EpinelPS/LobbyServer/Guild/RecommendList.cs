using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Guild
{
    [PacketPath("/guild/recommendlist")]
    public class GetRecommendList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqRecommendGuildList>();
            var response = new ResRecommendGuildList();


            await WriteDataAsync(response);
        }
    }
}

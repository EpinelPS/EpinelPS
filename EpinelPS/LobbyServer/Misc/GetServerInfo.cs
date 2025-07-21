using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/getserverinfo")]
    public class GetServerInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ResGetServerInfo r = new()
            {
                // todo: reimplement this as well
                MatchUrl = "https://global-match.nikke-kr.com",
                WorldId = 84
            };

            await WriteDataAsync(r);
        }
    }
}

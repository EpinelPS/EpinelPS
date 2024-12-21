using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/getserverinfo")]
    public class GetServerInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new ResGetServerInfo();

            // todo: reimplement this as well
            r.MatchUrl = "https://global-match.nikke-kr.com";
            r.WorldId = 84;

            await WriteDataAsync(r);
        }
    }
}

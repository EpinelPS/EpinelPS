using EpinelPS.Net;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Misc
{
    [PacketPath("/system/sentry/getparams")]
    public class GetSentryParams : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new SentryDataResponse();
            // TODO: Figure out a way so that the game developers would not be annoyed by bogus errors in Sentry dashboard

            await WriteDataAsync(r);
        }
    }
}

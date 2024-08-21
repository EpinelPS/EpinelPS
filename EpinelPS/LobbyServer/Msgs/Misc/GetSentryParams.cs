using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Misc
{
    [PacketPath("/system/sentry/getparams")]
    public class GetSentryParams : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var r = new ResGetSentryParams();
            // TODO: Figure out a way so that the game developers would not be annoyed by bogus errors in Sentry dashboard
            r.SamplingRate = 1E-06;
            r.TraceSamplingRate = 1E-06;
            await WriteDataAsync(r);
        }
    }
}

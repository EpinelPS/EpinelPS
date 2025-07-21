using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/system/sentry/getparams")]
    public class GetSentryParams : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ResGetSentryParams r = new()
            {
                // TODO: Figure out a way so that the game developers would not be annoyed by bogus errors in Sentry dashboard
                SamplingRate = 1E-06,
                TraceSamplingRate = 1E-06
            };
            await WriteDataAsync(r);
        }
    }
}

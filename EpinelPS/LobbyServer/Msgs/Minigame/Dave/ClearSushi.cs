using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/clearsushinew")]
    public class ClearDaveSushiNewIcon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearDaveSushiNewIcon>();

            var response = new ResClearDaveSushiNewIcon
            {

            };

            await WriteDataAsync(response);
        }
    }
}

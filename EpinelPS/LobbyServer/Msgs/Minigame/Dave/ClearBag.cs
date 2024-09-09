using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/clearbagnew")]
    public class ClearDaveBagNewIcon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqClearDaveBagNewIcon>();

            var response = new ResClearDaveBagNewIcon
            {

            };

            await WriteDataAsync(response);
        }
    }
}

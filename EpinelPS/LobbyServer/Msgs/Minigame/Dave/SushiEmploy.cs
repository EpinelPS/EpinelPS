using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/sushiemploy")]
    public class DaveSushiEmployee : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqDaveSushiEmployee>();

            var response = new ResDaveSushiEmployee
            {

            };

            await WriteDataAsync(response);
        }
    }
}

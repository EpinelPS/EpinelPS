using EpinelPS.Utils;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/driftbottle")]
    public class MiniGameDaveDriftBottle : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqMiniGameDaveDriftBottle>();

            var response = new ResMiniGameDaveDriftBottle
            {

            };

            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;
using Google.Protobuf.Collections;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/getalldavetrigger")]
    public class GetAllMiniGameDaveTriggers : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetAllMiniGameDaveTriggers>();

            var response = new ResGetAllMiniGameDaveTriggers
            {

            };

            await WriteDataAsync(response);
        }
    }
}

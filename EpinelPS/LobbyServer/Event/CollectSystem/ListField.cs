using EpinelPS.Utils;
using static EpinelPS.LobbyServer.Event.EventConstants;

namespace EpinelPS.LobbyServer.Event.CollectSystem
{
    [PacketPath("/event/collect-system/list-field")]
    public class ListField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListFieldEventCollectData>();
            var user = GetUser();

            var response = new ResListFieldEventCollectData();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

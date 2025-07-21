using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.CollectSystem
{
    [PacketPath("/event/collect-system/list-field")]
    public class ListField : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListFieldEventCollectData req = await ReadData<ReqListFieldEventCollectData>();
            User user = GetUser();

            ResListFieldEventCollectData response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

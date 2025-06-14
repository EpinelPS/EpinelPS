using EpinelPS.Utils;
using static EpinelPS.LobbyServer.Event.EventConstants;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/field/password-door/list")]
    public class ListPasswordDoor : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqListFieldPasswordDoorData>();
            var user = GetUser();

            var response = new ResListFieldPasswordDoorData();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

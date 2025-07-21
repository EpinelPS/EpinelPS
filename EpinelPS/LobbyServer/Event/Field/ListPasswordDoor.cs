using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/password-door/list")]
    public class ListPasswordDoor : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListFieldPasswordDoorData req = await ReadData<ReqListFieldPasswordDoorData>();
            User user = GetUser();

            ResListFieldPasswordDoorData response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

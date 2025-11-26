using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectevent")]
    public class SelectEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "location": { "chapter": 3, "stage": 1, "order": 3 }, "event": 111021115 }
            ReqSelectSimRoomEvent req = await ReadData<ReqSelectSimRoomEvent>();
            // User user = GetUser();

            ResSelectSimRoomEvent response = new()
            {
                Result = SimRoomResult.Success,
            };

            await WriteDataAsync(response);
        }
    }
}
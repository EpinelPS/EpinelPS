using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/quit")]
    public class Quit : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqQuitSimRoom>();
            var user = GetUser();

            ResQuitSimRoom response = new()
            {
                Result = SimRoomResult.SimRoomResultSuccess,
            };

            user.ResetableData.SimRoomData.Entered = false;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
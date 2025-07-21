using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/quit")]
    public class Quit : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqQuitSimRoom req = await ReadData<ReqQuitSimRoom>();
            User user = GetUser();

            ResQuitSimRoom response = new()
            {
                Result = SimRoomResult.Success,
            };

            user.ResetableData.SimRoomData.Entered = false;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
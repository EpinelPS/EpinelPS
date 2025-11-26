using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/simplemode/skipbuffselection")]
    public class SimpleModeSkipBuffSelection : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqSkipSimRoomSimpleModeBuffSelection>();
            User user = GetUser();
            ResSkipSimRoomSimpleModeBuffSelection response = new()
            {
                Result = SimRoomResult.Reset,
            };
            
            user.ResetableData.SimRoomData.Entered = false;

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
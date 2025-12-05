using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/popup-check/infinite")]
    public class InfinitePopupCheck : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqInfinitePopupCheck>();
            var user = GetUser();

            ResInfinitePopupCheck response = new();

            user.ResetableData.SimRoomData.CurrentSeasonData.WasInfinitePopupChecked = true;
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
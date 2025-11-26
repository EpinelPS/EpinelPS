using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/selectselectionevent")]
    public class SelectSelectionEvent : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSelectSimRoomSelectionEvent req = await ReadData<ReqSelectSimRoomSelectionEvent>();
            User user = GetUser();

            ResSelectSimRoomSelectionEvent response = new()
            {
                Result = SimRoomResult.Success
            };

            var location = req.Location;
            // Check
            var events = user.ResetableData.SimRoomData.Events;
            var simRoomEventIndex = events.FindIndex(x => x.Location.Chapter == location.Chapter && x.Location.Stage == location.Stage && x.Location.Order == location.Order);
            if (simRoomEventIndex < 0)
            {
                Logging.Warn("Not Fond UserSimRoomEvent");
                await WriteDataAsync(response);
            }
            SimRoomHelper.UpdateUserSimRoomEvent(user, index: simRoomEventIndex, events: events, selectionNumber: req.SelectionNumber);

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
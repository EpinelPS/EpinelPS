using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/proceedskipfunction")]
    public class ProceedSkipFunction : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            //  { "location": { "chapter": 3, "stage": 6, "order": 2 }, "event": 21060, "selectionNumber": 2, "selectionGroupElementId": 210602 }
            var req = await ReadData<ReqProceedSimRoomSkipFunction>();
            // ReqProceedSimRoomSkipFunction Fields
            //  NetSimRoomEventLocationInfo location,
            //  int event,
            //  int selectionNumber, 
            //  int selectionGroupElementId,
            User user = GetUser();
            // ResProceedSimRoomSkipFunction Fields
            //  SimRoomResult Result,
            ResProceedSimRoomSkipFunction response = new()
            {
                Result = SimRoomResult.Success
            };

            // Update 
            var location = req.Location;
            // Check
            var events = user.ResetableData.SimRoomData.Events;
            var simRoomEventIndex = events.FindIndex(x => x.Location.Chapter == location.Chapter && x.Location.Stage == location.Stage && x.Location.Order == location.Order);
            if (simRoomEventIndex < 0)
            {
                Logging.Warn("Not Fond UserSimRoomEvent");
                await WriteDataAsync(response);
            }
            SimRoomHelper.UpdateUserSimRoomEvent(user, index: simRoomEventIndex, events: events, selectionNumber: req.SelectionNumber, isDone: true);

            await WriteDataAsync(response);
        }
    }
}
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/getacquirebufffunction")]
    public class GetAcquireBuffFunction : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqGetSimRoomAcquireBuffFunction Fields
            //  NetSimRoomEventLocationInfo Location
            //  int Event
            //  int SelectionNumber
            //  int SelectionGroupElementId
            // { "location": { "chapter": 3, "stage": 6, "order": 2 }, "event": 22103, "selectionNumber": 2, "selectionGroupElementId": 221033 }
            var req = await ReadData<ReqGetSimRoomAcquireBuffFunction>();
            var user = GetUser();

            // ResGetSimRoomAcquireBuffFunction Fields
            //  SimRoomResult Result
            //  int RandomBuff
            ResGetSimRoomAcquireBuffFunction response = new()
            {
                Result = SimRoomResult.Success,
                RandomBuff = 2020406
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
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}
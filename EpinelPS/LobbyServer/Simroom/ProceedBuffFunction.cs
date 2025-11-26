using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/proceedbufffunction")]
    public class ProceedBuffFunction : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "location": { "chapter": 3, "stage": 6, "order": 1 }, "event": 22116, "selectionNumber": 2, "selectionGroupElementId": 221162, "buffToDelete": 2030608 }
            ReqProceedSimRoomBuffFunction req = await ReadData<ReqProceedSimRoomBuffFunction>();
            // ReqProceedSimRoomBuffFunction Field NetSimRoomEventLocationInfo location, int event, int selectionNumber, int selectionGroupElementId, int buffToDelete
            User user = GetUser();

            // ReqProceedSimRoomBuffFunction Field SimRoomResult Result, RepeatedField<int> AcquiredBuff, RepeatedField<int> DeletedBuff
            ResProceedSimRoomBuffFunction response = new()
            {
                Result = SimRoomResult.Success
            };
            if (req.BuffToDelete > 0)
            {
                response.DeletedBuff.Add(req.BuffToDelete);
            }

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
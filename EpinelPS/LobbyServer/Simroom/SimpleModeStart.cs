using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/simplemode/start")]
    public class SimpleModeStart : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqStartSimRoomSimpleMode>();
            User user = GetUser();

            // ResStartSimRoomSimpleMode Fields
            //  SimRoomResult Result
            //  RepeatedField<NetSimRoomEvent> Events
            //  NextSimpleModeBuffSelectionInfo

            ResStartSimRoomSimpleMode response = new()
            {
                Result = SimRoomResult.Success,
                // NextSimpleModeBuffSelectionInfo = new()
                // {
                //     BuffOptions = { user.ResetableData.SimRoomData.LegacyBuffs }
                // }
            };


            // Events
            try
            {
                user.ResetableData.SimRoomData.Entered = true;
                var simRoomEvents = SimRoomHelper.GetSimRoomEvents(user, 5, 3); // 5 = difficulty, 3 = stage
                if (simRoomEvents is not null)
                {
                    foreach (var simRoomEvent in simRoomEvents)
                    {
                        // Check if event is battle and is first order
                        if (simRoomEvent.EventCase == NetSimRoomEvent.EventOneofCase.Battle && simRoomEvent.Location.Order == 1)
                        {
                            var location = simRoomEvent.Location;
                            if (location is null) continue;
                            SimRoomHelper.GetBuffOptions(user, location);
                        }
                    }
                    JsonDb.Save();

                    var userSimRoomEvents = user.ResetableData.SimRoomData.Events;
                    if (userSimRoomEvents is not null)
                    {
                       simRoomEvents = [.. userSimRoomEvents.Select(SimRoomHelper.MToNet)];
                    }
                    // Add events to response
                    response.Events.AddRange(simRoomEvents);
                }
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"SimpleModeStart Events Exception: {ex.Message}", LogType.Error);
            }

            await WriteDataAsync(response);
        }
    }
}
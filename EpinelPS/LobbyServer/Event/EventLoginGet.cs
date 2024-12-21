using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/login/get")]
    public class EventLoginGet : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqLoginEventData>();
			var evid = req.EventId;
            var response = new ResLoginEventData(); // fields "EndDate", "DisableDate", "RewardHistories", "LastAttendance"

            response.EndDate = DateTime.Now.AddDays(13).Ticks;
            response.DisableDate = DateTime.Now.AddDays(13).Ticks;
			response.LastAttendance = new LoginEventAttendance
            {
                Day = 1, // Example day value, adjust as needed
                AttendanceDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks // Assign Ticks here
            };
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 1 } ); 
            await WriteDataAsync(response);
        }
    }
}

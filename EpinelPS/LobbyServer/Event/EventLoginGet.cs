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
                Day = 14, // Example day value, adjust as needed
                AttendanceDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks // Assign Ticks here
            };
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 1 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 2 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 3 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 4 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 5 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 6 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 7 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 8 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 9 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 10 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 11 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 12 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 13 } );
			response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = true, Day = 14 } );
			
            await WriteDataAsync(response);
        }
    }
}

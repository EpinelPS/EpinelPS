using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/login/get")]
    public class EventLoginGet : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqLoginEventData req = await ReadData<ReqLoginEventData>();
            int evId = req.EventId;
            ResLoginEventData response = new()
            {
                EndDate = DateTime.Now.AddDays(13).Ticks,
                DisableDate = DateTime.Now.AddDays(13).Ticks,
                LastAttendance = new LoginEventAttendance
                {
                    Day = 14, // Example day value, adjust as needed
                    AttendanceDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)).Ticks // Assign Ticks here
                }
            }; // fields "EndDate", "DisableDate", "RewardHistories", "LastAttendance"
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

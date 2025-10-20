using EpinelPS.Data;
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
            User user = GetUser();
            int evId = req.EventId;
            ResLoginEventData response = new()
            {
                EndDate = DateTime.Now.AddDays(13).Ticks,
                DisableDate = DateTime.Now.AddDays(13).Ticks,
                LastAttendance = new LoginEventAttendance()
            }; // fields "EndDate", "DisableDate", "RewardHistories", "LastAttendance"
            // Check if event exists
            if (!user.LoginEventInfo.TryGetValue(evId, out var loginEventData))
            {
                loginEventData = new LoginEventData();
                user.LoginEventInfo.Add(evId, loginEventData);
                JsonDb.Save();
            }
            // Populate response with event data
            int day = 1;
            GameData.Instance.LoginEventTable.Values.Where(ev => ev.EventId == evId).ToList().ForEach(ev =>
            {
                loginEventData.LastDay = day++;
                response.RewardHistories.Add(new LoginEventRewardHistory() { IsReceived = loginEventData.Days.Contains(ev.Day), Day = ev.Day });
            });

            response.LastAttendance.Day = loginEventData.LastDay;
            response.LastAttendance.AttendanceDate = loginEventData.LastDate;

            await WriteDataAsync(response);
        }
    }
}

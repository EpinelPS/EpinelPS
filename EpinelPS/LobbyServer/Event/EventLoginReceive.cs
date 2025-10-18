using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/login/receive")]
    public class EventLoginReceive : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqObtainLoginEventReward Fields: EventId, Day
            ReqObtainLoginEventReward req = await ReadData<ReqObtainLoginEventReward>();
            User user = GetUser();

            ResObtainLoginEventReward response = new();

            if (!user.LoginEventInfo.TryGetValue(req.EventId, out var loginEventData))
            {
                loginEventData = new LoginEventData();
                user.LoginEventInfo.Add(req.EventId, loginEventData);
            }

            GameData.Instance.LoginEventTable.Values.Where(ev => ev.EventId == req.EventId && ev.Day == req.Day).ToList().ForEach(ev =>
            {
                response.Reward = RewardUtils.RegisterRewardsForUser(user, ev.RewardId);
            });

            if (!loginEventData.Days.Contains(req.Day))
            {
                // loginEventData.Days.Add(req.Day);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/login/allreceive")]
    public class EventLoginReceiveAll : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqObtainLoginEventReward Fields: EventId, Day
            ReqObtainAllLoginEventReward req = await ReadData<ReqObtainAllLoginEventReward>();
            User user = GetUser();

            ResObtainAllLoginEventReward response = new();

            if (!user.LoginEventInfo.TryGetValue(req.EventId, out var loginEventData))
            {
                loginEventData = new LoginEventData();
                user.LoginEventInfo.Add(req.EventId, loginEventData);
            }
            response.Reward = new();
            NetRewardData rewardData = new();
            GameData.Instance.LoginEventTable.Values.Where(ev => ev.EventId == req.EventId).ToList().ForEach(ev =>
            {
                if (!loginEventData.Days.Contains(ev.Day))
                {
                    loginEventData.Days.Add(ev.Day);
                    RewardRecord reward = GameData.Instance.GetRewardTableEntry(ev.RewardId) ?? throw new Exception($"unknown reward Id {ev.RewardId}");
                    foreach (var item in reward.Rewards)
                    {
                        if (item.RewardType != RewardType.None)
                        {
                            RewardUtils.AddSingleObject(user, ref rewardData, item.RewardId, item.RewardType, item.RewardValue);
                        }
                    }
                }
            });

            response.Reward = rewardData;
            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
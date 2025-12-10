using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/mission/reward")]
    public class ObtainMissionReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // { "eventId": 20001, "dailyEventId": [ 200010105 ] }
            // ReqObtainEventMissionReward Fields
            //  int EventId
            //  RepeatedField<int> EventMissionIdList
            //  Google.Protobuf.WellKnownTypes.Timestamp RequestTimeStamp
            var req = await ReadData<ReqObtainEventMissionReward>();
            User user = GetUser();

            // ResObtainEventMissionReward Fields
            //  NetRewardData Reward
            //  ObtainEventMissionRewardResult Result
            ResObtainEventMissionReward response = new()
            {
                Result = ObtainEventMissionRewardResult.Success
            };

            var reward = new NetRewardData();
            try
            {
                EventMissionHelper.ObtainReward(user, ref reward, req.EventId, req.EventMissionIdList, req.RequestTimeStamp);
            }
            catch (Exception ex)
            {
                Logging.Warn($"ObtainMissionReward failed: {ex.Message}");
            }
            response.Reward = reward;

            await WriteDataAsync(response);
        }
    }
}

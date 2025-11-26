using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Simroom
{
    [PacketPath("/simroom/simplemode/skipall")]
    public class SimpleModeSkipAll : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqSkipAllSimRoomSimpleMode>();
            var user = GetUser();

            // ResSkipAllSimRoomSimpleMode Fields
            //  SimRoomResult Result
            //  NetRewardData Reward
            //  NetRewardData RewardByRewardUpEvent

            ResSkipAllSimRoomSimpleMode response = new()
            {
                Result = SimRoomResult.Success
            };

            // Reward
            try
            {
                user.ResetableData.SimRoomData.CurrentDifficulty = 5;
                user.ResetableData.SimRoomData.CurrentChapter = 3;
                var reward = SimRoomHelper.SimRoomReceivedReward(user, 5, 3); // 5 = difficulty, 3 = stage
                if (reward is not null) response.Reward = reward;
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"SkipAllSimpleMode Reward Exception: {ex.Message}", LogType.Error);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
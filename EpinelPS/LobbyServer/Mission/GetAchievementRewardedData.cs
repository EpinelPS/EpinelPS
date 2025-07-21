using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Mission
{
    [PacketPath("/mission/getrewarded/achievement")]
    public class GetAchievementRewardedData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            await ReadData<ReqGetAchievementRewardedData>();
            User user = GetUser();

            ResGetAchievementRewardedData response = new();
            response.Ids.AddRange(user.CompletedAchievements);
            
            await WriteDataAsync(response);
        }
    }
}

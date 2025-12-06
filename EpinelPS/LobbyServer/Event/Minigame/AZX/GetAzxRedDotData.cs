using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/get/reddot/data")]
    public class GetAzxRedDotData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqGetMiniGameAzxRedDotData Fields
            //  int AzxId
            ReqGetMiniGameAzxRedDotData req = await ReadData<ReqGetMiniGameAzxRedDotData>();
            User user = GetUser();
            
            // ResGetMiniGameAzxRedDotData Fields
            //  bool IsDailyMissionAvailable
            //  bool AchievementMissionRewardExists
            ResGetMiniGameAzxRedDotData response = new()
            {
                IsDailyMissionAvailable = true,
                AchievementMissionRewardExists = true
            };
            

            await WriteDataAsync(response);
        }
    }
}
using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Mission
{
    [PacketPath("/mission/getrewarded/achievement")]
    public class GetAchievementRewardedData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetAchievementRewardedData>();

            var response = new ResGetAchievementRewardedData();

            // TODO
            await WriteDataAsync(response);
        }
    }
}

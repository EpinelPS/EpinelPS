using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Ranking
{
    [PacketPath("/ranking/rankachievementtop")]
    public class GetRankAchieventTop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetRankAchievementTop>();

            var response = new ResGetRankAchievementTop();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

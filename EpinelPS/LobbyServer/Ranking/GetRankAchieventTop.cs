using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Ranking
{
    [PacketPath("/ranking/rankachievementtop")]
    public class GetRankAchieventTop : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetRankAchievementTop req = await ReadData<ReqGetRankAchievementTop>();

            ResGetRankAchievementTop response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.TowerDefense;

[GameRequest("/arcade/towerdefense/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeTowerDefenseRanking req = await ReadData<ReqGetArcadeTowerDefenseRanking>();
        User user = GetUser();
        ResGetArcadeTowerDefenseRanking response = new() { UserGuildRanking = new() };

        if (user.Guild.guildId > 0)
        {

            var allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, req.ArcadeManagerId);
            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    NetArcadeTowerDefenseRankingData? guild = new NetArcadeTowerDefenseRankingData()
                    {
                        Rank = item.Rank,
                        Score = item.Record.Score,
                        User = LobbyHandler.CreateWholeUserDataFromDbUser(user0)
                    };

                    response.GuildRankingList.Add(guild);
                    if (item.Record.UserId == user.ID)
                    {
                        response.UserGuildRanking = guild;
                    }

                }
            }
        }

       
        // TODO
        await WriteDataAsync(response);
    }
}
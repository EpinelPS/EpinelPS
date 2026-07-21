using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.Dessertrush;

[GameRequest("/arcade/dessertrush/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDessertRushRanking req = await ReadData<ReqGetArcadeDessertRushRanking>();
        User user = GetUser();
        ResGetArcadeDessertRushRanking response = new() {UserGuildRanking = new() 
        { Rank = 0,Score = 0 ,User = LobbyHandler.CreateWholeUserDataFromDbUser(user) } };

        if (user.Guild.guildId > 0)
        {
            IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, req.ArcadeId);

            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    NetArcadeDessertRushRankingData? guild = new NetArcadeDessertRushRankingData()
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
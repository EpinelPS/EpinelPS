using EpinelPS.Database;
using EpinelPS.Utils;
using System.Data.Common;


namespace EpinelPS.LobbyServer.Minigame.Bbq;

[GameRequest("/arcade/bbq/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBBQRanking req = await ReadData<ReqGetArcadeBBQRanking>();
        User user = GetUser();
        ResGetArcadeBBQRanking response = new ResGetArcadeBBQRanking() { UserGuildRanking = new() };

        if (user.Guild.guildId > 0)
        {

           IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, req.ArcadeId);

            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    GameUser? targetUser = GetUserNew(item.Record.UserId);
                    NetArcadeBBQRankingData? guild = new NetArcadeBBQRankingData()
                    {
                        Rank = item.Rank,
                        Score = item.Record.Score,
                        User = LobbyHandler.CreateWholeUserDataFromDbUser(targetUser)
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


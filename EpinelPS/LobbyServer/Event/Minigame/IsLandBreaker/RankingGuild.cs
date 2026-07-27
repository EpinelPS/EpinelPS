using EpinelPS.Data;
using EpinelPS.LobbyServer.Minigame;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.IsLandBreaker;

[GameRequest("/event/minigame/islandbreaker/ranking/guild")]
public class RankingGuild : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameIslandBreakerGuildRanking req = await ReadData<ReqGetMiniGameIslandBreakerGuildRanking>();
        User user = GetUser();
        ResGetMiniGameIslandBreakerGuildRanking response = new() { UserEntry = new()
        {
            User = LobbyHandler.CreateWholeUserDataFromDbUser(user)
        } };

        var manage = GameData.Instance.EventIslandBreakerManagerTable.Values
            .Where(x => x.MinigameType == MiniGameSystemType.Normal).FirstOrDefault();

        if (user.Guild.guildId > 0)
        {

            IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, manage.EventId,req.IslandBreakerId);

            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    var guild = new NetMiniGameIslandBreakerRanking()
                    {
                        Rank = item.Rank,
                        Score = item.Record.Score,
                        User = LobbyHandler.CreateWholeUserDataFromDbUser(user0)
                    };

                    response.Entries.Add(guild);
                    if (item.Record.UserId == user.ID)
                    {
                        response.UserEntry = guild;
                    }

                }
            }
        }

        // TODO
        await WriteDataAsync(response);
    }
}
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Minigame.BTG;

[GameRequest("/arcade/btg/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBtgRanking req = await ReadData<ReqGetArcadeBtgRanking>();
        User user = GetUser();
        ResGetArcadeBtgRanking response = new()
        {
            UserGuildRanking = new()
            {
                Rank = 0,
                Score = 0,
                User = LobbyHandler.CreateWholeUserDataFromDbUser(user)
            }
        };

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
            .Where(x => x.GameType == ArcadeGameType.BTG).FirstOrDefault();

        if (user.Guild.guildId > 0)
        {

            IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, arcade.Id,req.BtgId);

            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    var guild = new NetMiniGameBtgRankingData()
                    {
                        Rank = item.Rank,
                        Score = (int)item.Record.Score,
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
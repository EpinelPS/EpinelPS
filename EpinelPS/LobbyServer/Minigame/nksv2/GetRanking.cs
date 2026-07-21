using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMiniGameNKSV2Ranking req = await ReadData<ReqGetMiniGameNKSV2Ranking>();
        User user = GetUser();
        ResGetMiniGameNKSV2Ranking response = new() { UserGuildRanking = new() 
        {
            Rank = 0,
            Score = 0,
            User = LobbyHandler.CreateWholeUserDataFromDbUser(user)
        } };

        //Logging.WriteLine($"{req.NKsId}", LogType.Info);

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
            .Where(x => x.GameType == ArcadeGameType.BTG).FirstOrDefault();

        if (user.Guild.guildId > 0)
        {

            IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, arcade.Id, req.NKsId);

            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    var guild = new NetMiniGameNKSV2RankingData()
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
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Minigame.DragonDungeonRun;

[GameRequest("/arcade/ddr/union-ranking")]
public class UnionRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeDragonDungeonRunUnionRanking req = await ReadData<ReqGetArcadeDragonDungeonRunUnionRanking>();
        User user = GetUser();
        ResGetArcadeDragonDungeonRunUnionRanking response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.DragonDungeonRun).FirstOrDefault();

        if (user.Guild.guildId > 0)
        {
            IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId.Value, arcade.Id);

            if (allBoard.Count() > 0)
            {
                foreach (var item in allBoard)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    var guild = new NetMiniGameDragonDungeonRunRankingData()
                    {
                        Rank = item.Rank,
                        Score = (int)item.Record.Score,
                        User = LobbyHandler.CreateWholeUserDataFromDbUser(user0)
                    };
                    response.RankingDataList.Add(guild);

                    if (item.Record.UserId == user.ID)
                    {
                        response.UserRankingData = guild;
                    }

                }
            }
        }

        // TODO
        await WriteDataAsync(response);
    }
}
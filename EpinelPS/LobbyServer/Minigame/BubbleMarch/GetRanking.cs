using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/getranking")]
public class GetRanking : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeBubbleMarchRanking req = await ReadData<ReqGetArcadeBubbleMarchRanking>();
        User user = GetUser();
        ResGetArcadeBubbleMarchRanking response = new() { UserGuildRanking = new() 
        {
            User = LobbyHandler.CreateWholeUserDataFromDbUser(user)
        } };


        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();
        if (user.Guild.guildId > 0)
        {
            IEnumerable<(ArcadeScoreRecord Record, int Rank)>? allBoard = MiniGameHelper.GetFullLeaderboard((long)user.Guild.guildId, arcade.Id);
            if (allBoard.Any())
            {
                // 按 UserId 分组，取 ModeId 最大的记录
                var bestRecords = allBoard
                    .GroupBy(x => x.Record.UserId)
                    .Select(g => g.OrderByDescending(x => x.Record.ModeId).First())
                    .ToList();

                foreach (var item in bestRecords)
                {
                    User? user0 = GetUser(item.Record.UserId);
                    var guild = new NetMiniGameBubbleMarchRankingData()
                    {
                        Rank = item.Rank,
                        Wave = item.Record.ModeId,
                        WaveProgress = item.Record.Score,
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
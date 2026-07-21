using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.nksv2;

[GameRequest("/arcade/nksv2/accumulaterankingscore")]
public class AccumulateRankingScore : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAccumulateMiniGameNKSV2RankingScore req = await ReadData<ReqAccumulateMiniGameNKSV2RankingScore>();
        User user = GetUser();
        ResAccumulateMiniGameNKSV2RankingScore response = new();

        //Logging.WriteLine($"{req.NKsId},{req.Score}", LogType.Info);

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.NKSMiniGame).FirstOrDefault();

        if (user.Nksv2Datas.TryGetValue(req.NKsId, out var nksv2Data))
        {
            if (req.Score > nksv2Data.Score)
            {
                nksv2Data.Score = req.Score;
                if (user.Guild.guildId > 0)
                {
                    MiniGameHelper.InsertOrUpdate(arcade.Id, user.ID, user.Guild.guildId.Value, (int)req.Score, req.NKsId);
                }
            }

        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
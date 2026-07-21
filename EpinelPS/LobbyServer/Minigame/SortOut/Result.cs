using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.SortOut;

[GameRequest("/arcade/sortout/result")]
public class Result : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqArcadeSortOutResult req = await ReadData<ReqArcadeSortOutResult>();
        User user = GetUser();
        ResArcadeSortOutResult response = new();

        foreach (var item in req.PlayInfo.BoxSortOutCounts)
        {
            BoxSortOutCount? box = user.SortOutData.AccumulatedBoxSortOutCounts
                .Where(x => x.BoxId == item.BoxId).FirstOrDefault();
            if (box!=null)
            {
                box.Count += item.Count;
            }
            else
            {
                user.SortOutData.AccumulatedBoxSortOutCounts.Add(new()
                {
                    BoxId = item.BoxId,
                    Count = item.Count
                });
            }
        }

        int newscore = req.Point;
        if (newscore > user.SortOutData.HighScore)
        {
            user.SortOutData.HighScore = newscore;

            if (user.Guild.guildId > 0)
            {
                MiniGameHelper.InsertOrUpdate(req.ArcadeId, user.ID, user.Guild.guildId.Value, newscore, 0);
            }

            user.AddTrigger(Trigger.EventSortOutPointMax, newscore, 0);
        }
        user.SortOutData.TotalAccumulatedScore += newscore;
        user.AddTrigger(Trigger.EventSortOutClear, 1, 0);

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }  

}
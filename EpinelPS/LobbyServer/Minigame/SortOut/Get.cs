using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.SortOut;

[GameRequest("/arcade/sortout/get")]
public class Get : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetArcadeSortOut req = await ReadData<ReqGetArcadeSortOut>();
        User user = GetUser();
        ResGetArcadeSortOut response = new();

        var list = MiniGameHelper.ToProtoList<NetBoxSortOutCount, BoxSortOutCount>(user.SortOutData.AccumulatedBoxSortOutCounts);
        response.AccumulatedBoxSortOutCounts.AddRange(list);
        response.HighScore = user.SortOutData.HighScore;
        response.MissionRewarded.AddRange(user.SortOutData.MissionRewarded);
        response.TotalAccumulatedScore = user.SortOutData.TotalAccumulatedScore;

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
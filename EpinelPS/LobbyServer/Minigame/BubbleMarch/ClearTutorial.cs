using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Minigame.BubbleMarch;

[GameRequest("/arcade/bubblemarch/cleartutorial")]
public class ClearTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqClearArcadeBubbleMarchTutorial req = await ReadData<ReqClearArcadeBubbleMarchTutorial>();
        User user = GetUser();
        ResClearArcadeBubbleMarchTutorial response = new();

        ArcadeManagerRecord_Raw? arcade = GameData.Instance.ArcadeManagerTable.Values
             .Where(x => x.GameType == ArcadeGameType.BubbleMarch).FirstOrDefault();

        if (user.BubbleMarchDatas.TryGetValue(arcade.GameManagerId, out var marchData))
        {
            marchData.ClearedTutorialIdList.AddRangeUnique(req.TutorialIds);
        }
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
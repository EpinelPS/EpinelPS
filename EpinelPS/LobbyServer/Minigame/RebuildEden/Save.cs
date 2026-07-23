using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Minigame.RebuildEden;

[GameRequest("/arcade/rebuildeden/save")]
public class Save : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSaveRebuildEdenData req = await ReadData<ReqSaveRebuildEdenData>();
        User user = GetUser();
        ResSaveRebuildEdenData response = new();

        EventRebuildEdenManagerRecord_Raw? manger = GameData.Instance.EventRebuildEdenManagerTable.Values
            .Where(x => x.MinigameType == MiniGameSystemType.Arcade).FirstOrDefault();

        if (user.RebuildEdenDatas.TryGetValue(manger.Id, out var rebuildEden))
        {
            rebuildEden.Data = MiniGameHelper.FromProto<EdenData, NetRebuildEdenData>(req.Data);

            foreach (var item in req.DailyMissions)
            {
                rebuildEden.DailyMissions[item.MissionId] = item.Progress;
            }

            response.Result = MissionActionResult.Success;
        }

        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
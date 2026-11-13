using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event;

[GameRequest("/eventquest/obtain")]
public class ObtainEventQuestReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainEventQuestReward req = await ReadData<ReqObtainEventQuestReward>();
        User user = GetUser();

        foreach (int tid in req.EventQuestTidList)
        {
            user.EventQuestData[tid] = true;
        }

        JsonDb.Save();

        // TODO reward (event quest reward table is not loaded)
        await WriteDataAsync(new ResObtainEventQuestReward() { Reward = new NetRewardData() });
    }
}

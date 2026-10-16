namespace EpinelPS.LobbyServer.EventQuest;

[GameRequest("/eventquest/minigame/towerdefense/enter")]
public class EnterEventQuestMiniGameTowerDefense : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqEnterEventQuestMiniGameTowerDefense>();
        ResEnterEventQuestMiniGameTowerDefense response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/eventquest/minigame/towerdefense/finish")]
public class FinishEventQuestMiniGameTowerDefense : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqFinishEventQuestMiniGameTowerDefense>();
        ResFinishEventQuestMiniGameTowerDefense response = new()
        {
            Reward = new NetRewardData()
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/eventquest/minigame/mvg/enter")]
public class EnterEventQuestMiniGameMVG : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqEnterEventQuestMiniGameMVG>();
        ResEnterEventQuestMiniGameMVG response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/eventquest/minigame/mvg/finish")]
public class FinishEventQuestMiniGameMVG : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqFinishEventQuestMiniGameMVG>();
        ResFinishEventQuestMiniGameMVG response = new()
        {
            Reward = new NetRewardData()
        };
        await WriteDataAsync(response);
    }
}

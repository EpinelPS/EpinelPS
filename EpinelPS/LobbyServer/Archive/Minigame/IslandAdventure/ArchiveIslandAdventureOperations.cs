namespace EpinelPS.LobbyServer.Archive.Minigame.IslandAdventure;

[GameRequest("/archive/minigame/islandadventure/fish/collection")]
[GameRequest("/archive/minigame/islandadventure/get/fish/collection")]
[GameRequest("/minigame/islandadventure/fish/collection")]
[GameRequest("/minigame/islandadventure/get/fish/collection")]
public class ArchiveMiniGameIslandAdventureFishCollection : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveMiniGameIslandAdventureFishCollection>();
        ResArchiveMiniGameIslandAdventureFishCollection response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/get/rankings")]
[GameRequest("/archive/minigame/islandadventure/rankings")]
[GameRequest("/minigame/islandadventure/get/rankings")]
[GameRequest("/minigame/islandadventure/rankings")]
public class ArchiveMiniGameIslandAdventureRankings : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveGetMiniGameIslandAdventureRankings>();
        ResArchiveGetMiniGameIslandAdventureRankings response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/equip/item")]
[GameRequest("/minigame/islandadventure/equip/item")]
public class ArchiveEquipMiniGameIslandAdventureItem : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveEquipMiniGameIslandAdventureItem>();
        ResArchiveEquipMiniGameIslandAdventureItem response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/buy/item")]
[GameRequest("/minigame/islandadventure/buy/item")]
public class ArchiveBuyMiniGameIslandAdventureItem : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveBuyMiniGameIslandAdventureItem>();
        ResArchiveBuyMiniGameIslandAdventureItem response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/obtain/stepupreward")]
[GameRequest("/archive/minigame/islandadventure/obtain/stepup/reward")]
[GameRequest("/minigame/islandadventure/obtain/stepupreward")]
[GameRequest("/minigame/islandadventure/obtain/stepup/reward")]
public class ArchiveObtainMiniGameIslandAdventureStepUpReward : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveObtainMiniGameIslandAdventureStepUpReward>();
        ResArchiveObtainMiniGameIslandAdventureStepUpReward response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/complete/mission")]
[GameRequest("/minigame/islandadventure/complete/mission")]
public class ArchiveCompleteIslandAdventureMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveCompleteIslandAdventureMission>();
        ResArchiveCompleteIslandAdventureMission response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/take/photo")]
[GameRequest("/minigame/islandadventure/take/photo")]
public class ArchiveTakePhotoMiniGameIslandAdventure : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveTakePhotoMiniGameIslandAdventure>();
        ResArchiveTakePhotoMiniGameIslandAdventure response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/fishing")]
[GameRequest("/minigame/islandadventure/fishing")]
public class ArchiveFishingMiniGameIslandAdventure : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveFishingMiniGameIslandAdventure>();
        ResArchiveFishingMiniGameIslandAdventure response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/unequip/fishingbait")]
[GameRequest("/archive/minigame/islandadventure/fishingbait/unequip")]
[GameRequest("/minigame/islandadventure/unequip/fishingbait")]
[GameRequest("/minigame/islandadventure/fishingbait/unequip")]
public class ArchiveUnequipIslandAdventureFishingBait : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveUnequipIslandAdventureFishingBait>();
        ResArchiveUnequipIslandAdventureFishingBait response = new();
        await WriteDataAsync(response);
    }
}

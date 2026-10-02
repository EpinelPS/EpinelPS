namespace EpinelPS.LobbyServer.Archive.Minigame.IslandAdventure;

[GameRequest("/archive/minigame/islandadventure/get/fishing/stepupreward")]
[GameRequest("/minigame/islandadventure/get/fishing/stepupreward")]
public class ArchiveGetFishingStepUpRewardStatus : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveGetIslandAdventureFishingStepUpRewardStatus>();
        ResArchiveGetIslandAdventureFishingStepUpRewardStatus response = new()
        {
            FishAccumulatedScore = 0
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/get/photo/stepupreward")]
[GameRequest("/minigame/islandadventure/get/photo/stepupreward")]
public class ArchiveGetPhotoStepUpRewardStatus : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveGetIslandAdventurePhotoStepUpRewardStatus>();
        ResArchiveGetIslandAdventurePhotoStepUpRewardStatus response = new()
        {
            PhotoAccumulatedScore = 0
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/list/mission")]
[GameRequest("/minigame/islandadventure/list/mission")]
public class ArchiveListMission : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqGetArchiveIslandAdventureMissionData>();
        ResGetArchiveIslandAdventureMissionData response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/get/currency")]
[GameRequest("/minigame/islandadventure/get/currency")]
public class ArchiveMiniGameIslandAdventureCurrency : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveGetMiniGameIslandAdventureCurrency>();
        ResArchiveGetMiniGameIslandAdventureCurrency response = new()
        {
            Currency = 90000
        };
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/get/inventory")]
[GameRequest("/minigame/islandadventure/get/inventory")]
public class ArchiveMiniGameIslandAdventureInventory : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveGetMiniGameIslandAdventureInventory>();
        ResArchiveGetMiniGameIslandAdventureInventory response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/get/fish/spotcount")]
[GameRequest("/minigame/islandadventure/get/fish/spotcount")]
public class ArchiveMiniGameIslandAdventureFishingSpotCountHistory : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveMiniGameIslandAdventureFishingSpotCountHistory>();
        ResArchiveMiniGameIslandAdventureFishingSpotCountHistory response = new();
        await WriteDataAsync(response);
    }
}

[GameRequest("/archive/minigame/islandadventure/get/photo/album")]
[GameRequest("/minigame/islandadventure/get/photo/album")]
public class ArchiveMiniGameIslandAdventurePhotoAlbum : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        _ = await ReadData<ReqArchiveMiniGameIslandAdventurePhotoAlbum>();
        ResArchiveMiniGameIslandAdventurePhotoAlbum response = new();
        await WriteDataAsync(response);
    }
}

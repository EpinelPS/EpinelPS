using EpinelPS.Database;

namespace EpinelPS.LobbyServer.ContentsOpen;

[GameRequest("/contentsopen/get/unlock")]
public class GetUnlocked : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetContentsOpenUnlockInfo req = await ReadData<ReqGetContentsOpenUnlockInfo>();
        User user = GetUser();

        // This request is used for showing the "Collection Item Unlocked" Popup and button unlock animation

        ResGetContentsOpenUnlockInfo response = new();

        foreach (KeyValuePair<int, UnlockData> item in user.ContentsOpenUnlocked.OrderBy(x => x.Key))
        {
            response.ContentsOpenUnlockInfoList.Add(new NetContentsOpenUnlockInfo()
            {
                ContentsOpenTableId = item.Key,
                IsUnlockButtonPlayed = item.Value.ButtonAnimationPlayed,
                IsUnlockPopupPlayed = item.Value.PopupAnimationPlayed,
            });
        }

        await WriteDataAsync(response);
    }
}

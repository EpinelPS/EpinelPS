using EpinelPS.Database;
using EpinelPS.Data;

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

        int beforeCount = user.ContentsOpenUnlocked.Count;

        foreach (int contentsOpenId in GameData.Instance.ContentsOpenTable.Keys.Select(key => (int)key))
        {
            if (!user.ContentsOpenUnlocked.ContainsKey(contentsOpenId))
            {
                user.ContentsOpenUnlocked.Add(contentsOpenId, new(true, true));
            }
        }

        if (user.ContentsOpenUnlocked.Count == beforeCount && user.ContentsOpenUnlocked.Count == 0)
        {
            // These Always returned as true by official server
            // Fixes "Recruitment unlocked" during chapter 0
            // TODO: Don't hardcode this, maybe its in GameData

            user.ContentsOpenUnlocked.Add(3, new(true, true));
            user.ContentsOpenUnlocked.Add(4, new(true, true));
            user.ContentsOpenUnlocked.Add(6, new(true, true));
            user.ContentsOpenUnlocked.Add(15, new(true, true));
            user.ContentsOpenUnlocked.Add(16, new(true, true));
            user.ContentsOpenUnlocked.Add(18, new(true, true));
            user.ContentsOpenUnlocked.Add(19, new(true, true));
        }

        if (user.ContentsOpenUnlocked.Count != beforeCount)
        {
            JsonDb.Save();
        }

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

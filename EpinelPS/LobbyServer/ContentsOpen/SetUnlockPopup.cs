using EpinelPS.Database;

namespace EpinelPS.LobbyServer.ContentsOpen;

[GameRequest("/contentsopen/set/unlock/popup")]
public class SetUnlockPopup : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetContentsOpenUnlockPopupPlay req = await ReadData<ReqSetContentsOpenUnlockPopupPlay>();
        User user = GetUser();

        ResSetContentsOpenUnlockPopupPlay response = new();

        foreach (int item in req.ContentsOpenTableIds)
        {
            if (user.ContentsOpenUnlocked.TryGetValue(item, out UnlockData? data))
            {
                data.PopupAnimationPlayed = true;
            }
            else
            {
                user.ContentsOpenUnlocked.Add(item, new UnlockData()
                {
                    PopupAnimationPlayed = true
                });
            }
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}

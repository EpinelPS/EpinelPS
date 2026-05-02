using EpinelPS.Database;

namespace EpinelPS.LobbyServer.ContentsOpen;

[GameRequest("/contentsopen/set/unlock/button")]
public class SetUnlockButton : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetContentsOpenUnlockButtonPlay req = await ReadData<ReqSetContentsOpenUnlockButtonPlay>();
        User user = GetUser();

        ResSetContentsOpenUnlockButtonPlay response = new();

        // Unlock button animation completed

        foreach (int item in req.ContentsOpenTableIds)
        {
            if (user.ContentsOpenUnlocked.TryGetValue(item, out UnlockData? data))
            {
                data.ButtonAnimationPlayed = true;
            }
            else
            {
                user.ContentsOpenUnlocked.Add(item, new UnlockData()
                {
                    ButtonAnimationPlayed = true
                });
            }
        }

        JsonDb.Save();

        await WriteDataAsync(response);
    }
}

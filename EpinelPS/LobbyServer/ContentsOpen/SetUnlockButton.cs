using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.ContentsOpen
{
    [PacketPath("/contentsopen/set/unlock/button")]
    public class SetUnlockButton : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetContentsOpenUnlockButtonPlay>();
            var user = GetUser();

            var response = new ResSetContentsOpenUnlockButtonPlay();

            // Unlock button animation completed

            foreach (var item in req.ContentsOpenTableIds)
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
}

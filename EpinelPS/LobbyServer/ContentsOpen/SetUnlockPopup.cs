using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.ContentsOpen
{
    [PacketPath("/contentsopen/set/unlock/popup")]
    public class SetUnlockPopup : LobbyMsgHandler
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
}

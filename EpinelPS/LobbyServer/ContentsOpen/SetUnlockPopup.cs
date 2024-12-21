using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.ContentsOpen
{
    [PacketPath("/contentsopen/set/unlock/popup")]
    public class SetUnlockPopup : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetContentsOpenUnlockPopupPlay>();
            var user = GetUser();

            var response = new ResSetContentsOpenUnlockPopupPlay();

            foreach (var item in req.ContentsOpenTableIds)
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

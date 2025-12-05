using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.ContentsOpen
{
    [PacketPath("/contentsopen/get/unlock")]
    public class GetUnlocked : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetContentsOpenUnlockInfo req = await ReadData<ReqGetContentsOpenUnlockInfo>();
            User user = GetUser();

            // This request is used for showing the "Collection Item Unlocked" Popup and button unlock animation

            ResGetContentsOpenUnlockInfo response = new();

            if (user.ContentsOpenUnlocked.Count == 0)
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
}

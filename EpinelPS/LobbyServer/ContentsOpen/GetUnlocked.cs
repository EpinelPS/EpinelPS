using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.ContentsOpen
{
    [PacketPath("/contentsopen/get/unlock")]
    public class GetUnlocked : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetContentsOpenUnlockInfo>();
            var user = GetUser();

            var response = new ResGetContentsOpenUnlockInfo();

            // This request is used for showing the "Collection Item Unlocked" Popup and button unlock animation
            foreach (var item in user.ContentsOpenUnlocked)
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

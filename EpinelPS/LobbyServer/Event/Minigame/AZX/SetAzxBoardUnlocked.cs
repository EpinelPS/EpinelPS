using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/set/board/unlocked")]
    public class SetAzxBoardUnlocked : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqSetMiniGameAzxBoardUnlocked Fields
            //  int AzxId
            //  int BoardId
            ReqSetMiniGameAzxBoardUnlocked req = await ReadData<ReqSetMiniGameAzxBoardUnlocked>();
            User user = GetUser();

            ResSetMiniGameAzxBoardUnlocked response = new();

            if (req.BoardId > 0 && req.AzxId > 0)
                AzxHelper.SetBoardUnlocked(user, req.AzxId, req.BoardId);
            
            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
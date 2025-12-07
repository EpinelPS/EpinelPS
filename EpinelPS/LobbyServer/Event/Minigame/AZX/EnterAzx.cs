using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/enter")]
    public class EnterAzx : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // ReqEnterMiniGameAzx Fields
            //  int AzxId
            //  int PlayBoardId
            //  int PlayCharacterId
            ReqEnterMiniGameAzx req = await ReadData<ReqEnterMiniGameAzx>();
            User user = GetUser();

            // ResEnterMiniGameAzx Fields
            //  int PreviousSRankCount
            ResEnterMiniGameAzx response = new()
            {
                PreviousSRankCount = 0
            };

            if (req.AzxId > 0 && req.PlayBoardId > 0 && req.PlayCharacterId > 0)
                AzxHelper.EnterAzx(user, ref response, req.AzxId, req.PlayBoardId, req.PlayCharacterId);

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}
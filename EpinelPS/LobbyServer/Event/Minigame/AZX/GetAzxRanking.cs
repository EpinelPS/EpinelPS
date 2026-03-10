using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event.Minigame.AZX
{
    [PacketPath("/event/minigame/azx/get/ranking")]
    public class GetAzxRanking : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // int AzxId
            ReqGetMiniGameAzxRanking req = await ReadData<ReqGetMiniGameAzxRanking>();
            User user = User;

            ResGetMiniGameAzxRanking response = new();
            
            if (req.AzxId > 0)
                AzxHelper.GetRanking(user, req.AzxId, ref response);

            
            await WriteDataAsync(response);
        }
    }
}
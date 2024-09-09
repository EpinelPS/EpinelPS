using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Event
{
    [PacketPath("event/minigame/dave/getranking")]
    public class GetMiniGameDaveRanking : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameDaveRanking>();


            var response = new ResGetMiniGameDaveRanking
            {

            };

            await WriteDataAsync(response);
        }
    }
}

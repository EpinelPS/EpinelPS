using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Msgs.Archive
{
    [PacketPath("/archive/minigame/getdata")]
    public class GetMinigameData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetArchiveMiniGameData>();

            var response = new ResGetArchiveMiniGameData();

            response.Json = "{}";
            // TODO

            await WriteDataAsync(response);
        }
    }
}

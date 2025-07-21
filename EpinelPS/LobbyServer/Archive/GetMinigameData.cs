using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Archive
{
    [PacketPath("/archive/minigame/getdata")]
    public class GetMinigameData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetArchiveMiniGameData req = await ReadData<ReqGetArchiveMiniGameData>();

            ResGetArchiveMiniGameData response = new()
            {
                Json = "{}"
            };
            // TODO

            await WriteDataAsync(response);
        }
    }
}

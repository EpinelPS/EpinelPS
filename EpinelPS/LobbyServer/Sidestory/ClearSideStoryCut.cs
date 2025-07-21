using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Sidestory
{
    [PacketPath("/sidestory/cut/clearscenario")]
    public class ClearSideStoryCut : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearSideStoryCutForScenario req = await ReadData<ReqClearSideStoryCutForScenario>();
            Database.User user = GetUser();

            ResClearSideStoryCutForScenario response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

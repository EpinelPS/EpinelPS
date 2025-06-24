using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/counsel")]
    public class Counsel : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqCharacterCounsel>();
            var user = GetUser();

            ResCharacterCounsel response = new();
            response.Attractive = new();
            response.Exp = new();

            // TODO
            await WriteDataAsync(response);
        }
    }
}

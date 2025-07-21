using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate
{
    [PacketPath("/liberate/get")]
    public class GetLiberateData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetLiberateData req = await ReadData<ReqGetLiberateData>();
            Database.User user = GetUser();

            ResGetLiberateData response = new() { };

            // TODO

            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate
{
    [PacketPath("/liberate/getprogresslist")]
    public class GetProgressList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetLiberateProgressList req = await ReadData<ReqGetLiberateProgressList>();
            Database.User user = GetUser();

            ResGetLiberateProgressList response = new();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Liberate
{
    [PacketPath("/liberate/getprogresslist")]
    public class GetProgressList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetLiberateProgressList>();
            var user = GetUser();

            var response = new ResGetLiberateProgressList();

            // TODO

            await WriteDataAsync(response);
        }
    }
}

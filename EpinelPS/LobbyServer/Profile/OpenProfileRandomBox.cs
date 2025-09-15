using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Profile
{
    [PacketPath("/ProfileCard/ProfileRandomBox/Open")]
    public class OpenProfileRandomBox : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqOpenProfileRandomBox req = await ReadData<ReqOpenProfileRandomBox>();
            User user = GetUser();

            ResOpenProfileRandomBox response = new();


            await WriteDataAsync(response);
        }
    }
}
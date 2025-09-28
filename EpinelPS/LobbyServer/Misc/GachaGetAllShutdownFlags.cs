using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Misc
{
    [PacketPath("/shutdownflags/gacha/getall")]
    public class GachaGetAllShutdownFlags : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGachaGetAllShutdownFlags req = await ReadData<ReqGachaGetAllShutdownFlags>();
            User user = GetUser();

            ResGachaGetAllShutdownFlags response = new();
            if (user.GachaTutorialPlayCount > 0)
                response.Unavailables.Add(3);

            // TODO: ValIdate response from real server and pull info from user info
            await WriteDataAsync(response);
        }
    }
}

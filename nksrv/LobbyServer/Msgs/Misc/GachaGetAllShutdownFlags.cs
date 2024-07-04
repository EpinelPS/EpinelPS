using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Misc
{
    [PacketPath("/shutdownflags/gacha/getall")]
    public class GachaGetAllShutdownFlags : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGachaGetAllShutdownFlags>();
            var user = GetUser();

            var response = new ResGachaGetAllShutdownFlags();
            if (user.GachaTutorialPlayCount > 0)
                response.Unavailables.Add(3);

           // TODO: Validate response from real server and pull info from user info
           WriteData(response);
        }
    }
}

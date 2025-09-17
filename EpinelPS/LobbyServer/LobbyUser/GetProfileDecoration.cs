using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/DecorationLayout/Get")]
    public class GetProfileDecoration : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqProfileCardDecorationLayout req = await ReadData<ReqProfileCardDecorationLayout>();
            User user = GetUser();

            ResProfileCardDecorationLayout r = new()
            {
                Layout = user.ProfileCardDecoration.Layout
            };
            await WriteDataAsync(r);
        }
    }
}

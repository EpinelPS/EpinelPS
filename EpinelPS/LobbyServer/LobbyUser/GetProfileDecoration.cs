using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/DecorationLayout/Get")]
    public class GetProfileDecoration : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqProfileCardDecorationLayout req = await ReadData<ReqProfileCardDecorationLayout>();

            ResProfileCardDecorationLayout r = new()
            {
                Layout = new ProfileCardDecorationLayout
                {
                    BackgroundId = 101002,
                    ShowCharacterSpine = true
                }
            };
            await WriteDataAsync(r);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/Possession/Get")]
    public class GetProfileCardPossession : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqProfileCardObjectList req = await ReadData<ReqProfileCardObjectList>();

            ResProfileCardObjectList response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}

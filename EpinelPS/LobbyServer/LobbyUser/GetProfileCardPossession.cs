using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/Possession/Get")]
    public class GetProfileCardPossession : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqProfileCardObjectList>();

            var response = new ResProfileCardObjectList();
            // TODO
            await WriteDataAsync(response);
        }
    }
}

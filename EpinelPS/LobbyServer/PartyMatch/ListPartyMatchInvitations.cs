using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.PartyMatch
{
    [PacketPath("/partymatch/listinvitation")]
    public class ListPartyMatchInvitations : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListInvitation req = await ReadData<ReqListInvitation>();
            User user = User;

            ResListInvitation response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.PartyMatch
{
    [PacketPath("/partymatch/listinvitation")]
    public class ListPartyMatchInvitations : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListInvitation req = await ReadData<ReqListInvitation>();
            Database.User user = GetUser();

            ResListInvitation response = new();
            // TODO
            await WriteDataAsync(response);
        }
    }
}

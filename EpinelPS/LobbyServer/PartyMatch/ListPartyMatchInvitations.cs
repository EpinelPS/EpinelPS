namespace EpinelPS.LobbyServer.PartyMatch;

[GameRequest("/partymatch/listinvitation")]
public class ListPartyMatchInvitations : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqListInvitation req = await ReadData<ReqListInvitation>();
        User user = GetUser();

        ResListInvitation response = new();
        // TODO
        await WriteDataAsync(response);
    }
}

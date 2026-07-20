namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/inappshop/jupiter/issueactivitypoints")]
public class IssueActivityPoints : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqIssueJupiterActivityPoints req = await ReadData<ReqIssueJupiterActivityPoints>();
        User user = GetUser();

        ResIssueJupiterActivityPoints response = new()
        {
            IssueDetail = "",
            HasActivity = false
        };

        await WriteDataAsync(response);
    }
}

using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Shop;

[GameRequest("/inappshop/jupiter/issueactivitypoints")]
public class IssueActivityPoints : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqIssueJupiterActivityPoints req = await ReadData<ReqIssueJupiterActivityPoints>();
        User user = GetUser();

        Logging.WriteLine($"[InAppShop] /inappshop/jupiter/issueactivitypoints called by user {user.Nickname}", LogType.Debug);

        ResIssueJupiterActivityPoints response = new()
        {
            IssueDetail = "",
            HasActivity = false
        };

        await WriteDataAsync(response);
    }
}

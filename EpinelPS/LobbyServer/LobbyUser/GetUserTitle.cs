using EpinelPS.Data; // For GameData access

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/lobby/usertitle/get")]
public class GetUserTitle : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetUserTitleList req = await ReadData<ReqGetUserTitleList>();
        ResGetUserTitleList r = new();

        // Access GameData and get all UserTitle IDs
        Dictionary<int, UserTitleRecord> userTitleRecords = GameData.Instance.userTitleRecords;

        foreach (int titleId in userTitleRecords.Keys)
        {
            r.UserTitleList.Add(new ResGetUserTitleList.Types.NetUserTitle() { UserTitleId = titleId });
        }

        await WriteDataAsync(r);
    }
}

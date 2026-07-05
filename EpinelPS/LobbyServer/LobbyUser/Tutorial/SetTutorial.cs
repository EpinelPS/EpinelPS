using EpinelPS.Data;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.LobbyUser.Tutorial;

[GameRequest("/tutorial/set")]
public class SetTutorial : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqSetTutorial req = await ReadData<ReqSetTutorial>();
        User user = GetUser();

        var cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);
        var tutorial = new ClearedTutorialData()
        {
            Id = cleared.Id,
            VersionGroup = cleared.VersionGroup
        };

        if (!user.ClearedTutorialData.ContainsKey(cleared.GroupId))
        {
            user.ClearedTutorialData.Add(cleared.GroupId, tutorial);
        }
        else
        {
            user.ClearedTutorialData[cleared.GroupId] = tutorial;
        }
        await GameContext.SaveChangesAsync();

        ResSetTutorial response = new();
        await WriteDataAsync(response);
    }
}

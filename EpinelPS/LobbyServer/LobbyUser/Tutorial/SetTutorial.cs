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
        GameUser gameUser = GetUserNew();

        var cleared = GameData.Instance.GetTutorialDataById(req.LastClearedTid);
        var tutorial = new ClearedTutorialData()
        {
            Id = cleared.Id,
            VersionGroup = cleared.VersionGroup
        };

        if (!gameUser.ClearedTutorialData.ContainsKey(cleared.GroupId))
        {
            gameUser.ClearedTutorialData.Add(cleared.GroupId, tutorial);
        }
        else
        {
            gameUser.ClearedTutorialData[cleared.GroupId] = tutorial;
        }
        await GameContext.SaveChangesAsync();

        ResSetTutorial response = new();
        await WriteDataAsync(response);
    }
}

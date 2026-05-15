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

        if (!user.ClearedTutorialDataNew.ContainsKey(cleared.GroupId))
        {
            user.ClearedTutorialDataNew.Add(cleared.GroupId, tutorial);
        }
        else
        {
            user.ClearedTutorialDataNew[cleared.GroupId] = tutorial;
        }
        JsonDb.Save();

        ResSetTutorial response = new();
        await WriteDataAsync(response);
    }
}

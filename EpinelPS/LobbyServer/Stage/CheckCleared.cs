namespace EpinelPS.LobbyServer.Stage;

[GameRequest("/stage/checkclear")]
public class CheckCleared : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqCheckStageClear req = await ReadData<ReqCheckStageClear>();

        ResCheckStageClear response = new();
        User user = GetUser();

        foreach (var fields in user.FieldInfo)
        {
            foreach (int stages in fields.CompletedStages)
            {
                if (req.StageIds.Contains(stages))
                    response.ClearedStageIds.Add(stages);
            }
        }

        await WriteDataAsync(response);
    }
}

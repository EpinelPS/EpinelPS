namespace EpinelPS.LobbyServer.TriggerController;

[GameRequest("/Trigger/GetMainQuestData")]
public class GetMainQuestData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetMainQuestData req = await ReadData<ReqGetMainQuestData>();
        User user = GetUser();

        ResGetMainQuestData response = new();
        foreach (KeyValuePair<int, bool> item in user.MainQuestData)
        {
            response.MainQuestList.Add(new NetMainQuestData() { IsReceived = item.Value, Tid = item.Key });
        }

        await WriteDataAsync(response);
    }
}

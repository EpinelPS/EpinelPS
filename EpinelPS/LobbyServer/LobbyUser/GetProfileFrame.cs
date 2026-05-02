using EpinelPS.Data;

namespace EpinelPS.LobbyServer.LobbyUser;

[GameRequest("/User/GetProfileFrame")]
public class GetProfileFrame : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqGetProfileFrame req = await ReadData<ReqGetProfileFrame>();
        ResGetProfileFrame response = new();

        foreach (var frameRecord in GameData.Instance.userFrameTable.Values)
        {
            response.Frames.Add(frameRecord.Id);
        }

        await WriteDataAsync(response);
    }
}

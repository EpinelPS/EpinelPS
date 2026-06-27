namespace EpinelPS.LobbyServer.Antibot;

[GameRequest("/antibot/recvdata")]
public class ReceiveAntibotData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAntibotRecvData req = await ReadData<ReqAntibotRecvData>();

        // I don't really care about reimplementing the server side anticheat, so return

        ResAntibotRecvData response = new();

        await WriteDataAsync(response);
    }
}

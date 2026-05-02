namespace EpinelPS.LobbyServer.Antibot;

[GameRequest("/antibot/battlereportdata")]
public class BattleReportData : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqBattleReportData req = await ReadData<ReqBattleReportData>();
        ResBattleReportData response = new();

        // this is responsible for server side anticheat

        await WriteDataAsync(response);
    }
}

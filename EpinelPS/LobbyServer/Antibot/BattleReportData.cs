using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Antibot
{
    [PacketPath("/antibot/battlereportdata")]
    public class BattleReportData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBattleReportData req = await ReadData<ReqBattleReportData>();
            ResBattleReportData response = new();

            // this is responsible for server side anticheat

            await WriteDataAsync(response);
        }
    }
}

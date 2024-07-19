using nksrv.Utils;

namespace nksrv.LobbyServer.Msgs.Antibot
{
    [PacketPath("/antibot/battlereportdata")]
    public class BattleReportData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqBattleReportData>();
            var response = new ResBattleReportData();

            // this is responsible for server side anticheat

            await WriteDataAsync(response);
        }
    }
}

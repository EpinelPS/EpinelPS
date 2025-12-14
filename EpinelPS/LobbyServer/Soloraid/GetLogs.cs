using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Soloraid
{
    [PacketPath("/soloraid/getlogs")]
    public class GetLogs : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // int RaidId, int RaidLevel
            var req = await ReadData<ReqGetSoloRaidLogs>();
            var user = GetUser();
            ResGetSoloRaidLogs response = new();

            try
            {
                SoloRaidHelper.GetSoloRaidLog(user, ref response, req.RaidId, req.RaidLevel);
            }
            catch (Exception ex)
            {
                Logging.WriteLine($"GetLogs Error: {ex.Message}", LogType.Error);
            }

            await WriteDataAsync(response);
        }
    }
}

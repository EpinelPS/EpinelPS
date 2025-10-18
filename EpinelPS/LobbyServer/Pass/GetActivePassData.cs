using EpinelPS.Data;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/getactive")]
    public class GetActivePassData : LobbyMsgHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GetActivePassData));
        protected override async Task HandleAsync()
        {
            ReqGetActivePassData req = await ReadData<ReqGetActivePassData>();
            User user = GetUser();

            ResGetActivePassData response = new()
            {
                PassExist = false,
            };

            var passManager = GameData.Instance.PassManagerTable.Values.FirstOrDefault(p => p.SeasonStartDate <= DateTime.UtcNow && p.SeasonEndDate >= DateTime.UtcNow);
            if (passManager != null)
            {
                log.Debug($"Found active pass: {JsonConvert.SerializeObject(passManager)}");
                NetPassInfo passInfo = PassHelper.GetPassInfo(user, passManager.Id, passManager.PassPointId);
                // Simple validation to ensure we have a valid pass
                if (passInfo.PassId != 0 && passInfo.PassRankList.Count > 0)
                {
                    response.PassExist = true;
                    response.Pass = passInfo;
                }
            }
            else
            {
                Logging.WriteLine("No active pass found.");
            }

            await WriteDataAsync(response);
        }
    }
}

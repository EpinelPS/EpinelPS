using EpinelPS.Data;
using EpinelPS.LobbyServer.Event;
using EpinelPS.Utils;
using log4net;
using Newtonsoft.Json;

namespace EpinelPS.LobbyServer.Pass
{
    [PacketPath("/pass/event/getactive")]
    public class GetActiveEventPassData : LobbyMsgHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(GetActiveEventPassData));
        protected override async Task HandleAsync()
        {
            ReqGetActiveEventPassData req = await ReadData<ReqGetActiveEventPassData>(); // no fields
            User user = GetUser();

            ResGetActiveEventPassData response = new(); // fields PassList = NetPassInfo

            List<LobbyPrivateBannerRecord> lobbyPrivateBanners = [];//[.. GameData.Instance.LobbyPrivateBannerTable.Values.Where(b => b.PrivateBannerShowDuration <= DateTime.UtcNow && b.EndDate >= DateTime.UtcNow)];
            lobbyPrivateBanners = EventHelper.GetLobbyPrivateBannerData(user);
            // TODO: PrivateBannerShowDuration
            log.Debug($"Active lobby private banners: {JsonConvert.SerializeObject(lobbyPrivateBanners)}");
            if (lobbyPrivateBanners.Count <= 0)
            {
                // No active lobby private banners
                Logging.WriteLine("No active lobby private banners found.", LogType.Warning);
                await WriteDataAsync(response);
                return;
            }

            List<int> passIds = [];
            foreach (var banner in lobbyPrivateBanners)
            {
                passIds.AddRange(GameData.Instance.eventManagers.Values.Where(em => em.SetField == banner.EventId && em.EventSystemType == EventSystemType.EventPass).Select(em => em.Id));
            }
            log.Debug($"Active event pass IDs from banners: {JsonConvert.SerializeObject(passIds)}");
            if (passIds.Count == 0)
            {
                Logging.WriteLine("No active event pass IDs found from lobby private banners.", LogType.Warning);
                await WriteDataAsync(response);
                return;
            }

            var passManager = GameData.Instance.EventPassManagerTable.Values.Where(p => passIds.Contains(p.EventId)).ToList();
            log.Debug($"Active event pass managers: {JsonConvert.SerializeObject(passManager)}");
            if (passManager.Count == 0)
            {
                Logging.WriteLine("No active event pass found.");
                await WriteDataAsync(response);
                return;
            }
            
            foreach (var pm in passManager)
            {
                NetPassInfo passInfo = PassHelper.GetPassInfo(user, pm.Id, pm.PassPointId);
                if (passInfo.PassId != 0 && passInfo.PassRankList.Count > 0)
                {
                    response.PassList.Add(passInfo);
                }
            }

            await WriteDataAsync(response);
        }
    }
}

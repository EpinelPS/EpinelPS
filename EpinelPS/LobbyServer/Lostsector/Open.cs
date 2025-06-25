using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/open")]
    public class Open : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqOpenLostSector>();
            var user = GetUser();

            var response = new ResOpenLostSector();

            if (!user.LostSectorData.ContainsKey(req.SectorId))
                user.LostSectorData.Add(req.SectorId, new Database.LostSectorData()
                {
                    IsOpen = true
                });

            response.Lostsector = new NetUserLostSectorData()
            {
                IsOpen = true,
                SectorId = req.SectorId,
            };

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

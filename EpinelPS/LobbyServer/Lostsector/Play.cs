using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/play")]
    public class Play : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqPlayLostSector>();
            var user = GetUser();

            var response = new ResPlayLostSector();

            if (!user.LostSectorData.ContainsKey(req.SectorId))
                user.LostSectorData.Add(req.SectorId, new Database.LostSectorData()
                {
                    IsPlaying = true
                });

            var lostSectorData = user.LostSectorData[req.SectorId];
            lostSectorData.IsPlaying = true;

            response.Lostsector = new NetUserLostSectorData()
            {
                IsOpen = lostSectorData.IsOpen,
                SectorId = req.SectorId,
                IsPlaying = lostSectorData.IsPlaying
            };

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/play")]
    public class Play : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqPlayLostSector req = await ReadData<ReqPlayLostSector>();
            User user = GetUser();

            ResPlayLostSector response = new();

            if (!user.LostSectorData.ContainsKey(req.SectorId))
                user.LostSectorData.Add(req.SectorId, new Database.LostSectorData()
                {
                    IsPlaying = true
                });

            LostSectorData lostSectorData = user.LostSectorData[req.SectorId];
            lostSectorData.IsPlaying = true;

            response.Lostsector = new NetUserLostSectorData()
            {
                IsOpen = lostSectorData.IsOpen,
                SectorId = req.SectorId,
                IsPlaying = lostSectorData.IsPlaying,
                CurrentClearStageCount = lostSectorData.ClearedStages.Count,
                RewardCount = lostSectorData.ObtainedRewards,
                IsFinalReward = lostSectorData.RecievedFinalReward,
                IsPerfectReward = lostSectorData.CompletedPerfectly,
                MaxClearStageCount = 0, // TODO
            };

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

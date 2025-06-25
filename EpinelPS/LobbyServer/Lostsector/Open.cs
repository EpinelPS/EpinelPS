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
                user.LostSectorData.Add(req.SectorId, new LostSectorData()
                {
                    IsOpen = true
                });

            var val = user.LostSectorData[req.SectorId];
            response.Lostsector = new NetUserLostSectorData()
            {
                IsOpen = val.IsOpen,
                SectorId = req.SectorId,
                IsPlaying = val.IsPlaying,
                CurrentClearStageCount = val.ClearedStages.Count,
                RewardCount = val.ObtainedRewards,
                IsFinalReward = val.RecievedFinalReward,
                IsPerfectReward = val.CompletedPerfectly,
                MaxClearStageCount = 0, // TODO
            };

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}

using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Lostsector
{
    [PacketPath("/lostsector/open")]
    public class Open : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqOpenLostSector req = await ReadData<ReqOpenLostSector>();
            User user = GetUser();

            ResOpenLostSector response = new();

            if (!user.LostSectorData.ContainsKey(req.SectorId))
                user.LostSectorData.Add(req.SectorId, new LostSectorData()
                {
                    IsOpen = true
                });

            LostSectorData val = user.LostSectorData[req.SectorId];
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

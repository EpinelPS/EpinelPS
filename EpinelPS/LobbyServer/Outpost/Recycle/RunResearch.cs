using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Outpost.Recycle
{
    [PacketPath("/outpost/RecycleRoom/RunResearch")]
    public class RunResearch : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqRecycleRunResearch req = await ReadData<ReqRecycleRunResearch>();
            User user = GetUser();
            ResRecycleRunResearch response = new();

            user.ResearchProgress.TryGetValue(req.Tid, out RecycleRoomResearchProgress? progress);

            // Check progress is null, non-null means research is already unlocked.
            if (progress is null)
            {
                RecycleResearchStatRecord researchRecord = GameData.Instance.RecycleResearchStats.Values.FirstOrDefault(e => e.Id == req.Tid)
                    ?? throw new Exception("not found research record with tId " + req.Tid);
                progress = new()
                {
                    Attack = researchRecord.Attack,
                    Defense = researchRecord.Defence,
                    Hp = researchRecord.Hp,
                };
                user.ResearchProgress.Add(req.Tid, progress);
            }
            response.Recycle = new()
            {
                Tid = req.Tid,
                Lv = progress.Level,
                Exp = progress.Exp,
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

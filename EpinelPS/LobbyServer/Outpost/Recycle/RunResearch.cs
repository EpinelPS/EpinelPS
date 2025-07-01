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
            var req = await ReadData<ReqRecycleRunResearch>();
            var user = GetUser();
            var response = new ResRecycleRunResearch();

            user.ResearchProgress.TryGetValue(req.Tid, out var progress);

            // Check progress is null, non-null means research is already unlocked.
            if (progress is null)
            {
                var researchRecord = GameData.Instance.RecycleResearchStats.Values.FirstOrDefault(e => e.id == req.Tid)
                    ?? throw new Exception("not found research record with tid " + req.Tid);
                progress = new()
                {
                    Attack = researchRecord.attack,
                    Defense = researchRecord.defense,
                    Hp = researchRecord.hp,
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

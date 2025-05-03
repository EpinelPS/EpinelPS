using EpinelPS.Data;
using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Subquest
{
    [PacketPath("/subquest/enrollment")]
    public class EnrollSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnrollmentSubQuest>();
            var user = GetUser();

            var response = new ResEnrollmentSubQuest();

            if (!GameData.Instance.Subquests.TryGetValue(req.SubquestId, out _))
                throw new Exception("no such subquest: " + req.SubquestId);

            if (!user.SubQuestData.ContainsKey(req.SubquestId))
                user.SetSubQuest(req.SubquestId, false);

            response.SubquestData = new NetSubQuestData()
            {
                CreatedAt = DateTime.UtcNow.Ticks,
                IsReceived = false,
                SubQuestId = req.SubquestId
            };

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

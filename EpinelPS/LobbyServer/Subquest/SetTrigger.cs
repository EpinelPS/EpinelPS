using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Subquest
{
    [PacketPath("/subquest/settrigger")]
    public class SetTrigger : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetTriggerFromSubQuest req = await ReadData<ReqSetTriggerFromSubQuest>();
            User user = GetUser();

            ResSetTriggerFromSubQuest response = new();

            if (!GameData.Instance.Subquests.TryGetValue(req.SubquestId, out SubQuestRecord? record))
                throw new Exception("no such subquest: " + req.SubquestId);

            user.AddTrigger(record.ClearTrigger, record.ClearConditionValue, record.ClearConditionId);
            user.AddTrigger(Trigger.SubQuestClear, 1, req.SubquestId); // TODO move elsewhere?


            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

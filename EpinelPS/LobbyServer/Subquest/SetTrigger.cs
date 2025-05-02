using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Subquest
{
    [PacketPath("/subquest/settrigger")]
    public class SetTrigger : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSetTriggerFromSubQuest>();
            var user = GetUser();

            var response = new ResSetTriggerFromSubQuest();

            if (!GameData.Instance.Subquests.TryGetValue(req.SubquestId, out SubquestRecord? record))
                throw new Exception("no such subquest: " + req.SubquestId);


            user.AddTrigger(TriggerType.CampaignGroupClear, record.clear_condition_value, record.clear_condition_id); // TODO this may need to go elsewhere
user.AddTrigger(TriggerType.FieldObjectCollection, record.clear_condition_value, record.clear_condition_id); // TODO this may need to go elsewhere
            user.AddTrigger(TriggerType.SubQuestClear, 1, req.SubquestId);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}

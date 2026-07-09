using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Campaign;

[GameRequest("/campaign/obtain/item")]
public class ObtainItem : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainCampaignItem req = await ReadData<ReqObtainCampaignItem>();
        User user = GetUser();

        ResObtainCampaignItem response = new();

        var field = user.FieldInfo.FirstOrDefault(f => f.MapName == req.MapId);

        if (field == null)
        {
            field = new FieldInfoNew
            {
                MapName = req.MapId
            };
            user.FieldInfo.Add(field);
        }

        foreach (var item in field.CompletedObjects)
        {
            if (item.PositionId == req.FieldObject.PositionId)
            {
                Logging.WriteLine("attempted to collect campaign field object twice!", LogType.WarningAntiCheat);
                return;
            }
        }

        // Register and return reward
        var map = GameData.Instance.MapData[req.MapId];

        var position = map.ItemSpawner.Where(x => x.PositionId == req.FieldObject.PositionId).FirstOrDefault() ?? throw new Exception("bad position Id");

        FieldItemRecord positionReward = GameData.Instance.FieldItems[position.ItemId];
        RewardRecord reward = GameData.Instance.GetRewardTableEntry(positionReward.TypeValue) ?? throw new Exception("failed to get reward");
        response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);

        // HIde it from the field
        field.CompletedObjects.Add(new CompletedFieldObject() { PositionId = req.FieldObject.PositionId, Type = req.FieldObject.Type, ActionAt = DateTime.UtcNow,
        Json = req.FieldObject.Json, UserId = user.ID });

        await GameContext.SaveChangesAsync();

        await WriteDataAsync(response);
    }
}

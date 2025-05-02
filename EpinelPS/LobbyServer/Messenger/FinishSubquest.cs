using EpinelPS.Database;
using EpinelPS.StaticInfo;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/finsubquest")]
    public class FinishSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFinSubQuest>();
            var user = GetUser();

            var response = new ResFinSubQuest();

            var opener = GameData.Instance.Subquests.Where(x => x.Key == req.SubQuestId).First();
            var conversation = GameData.Instance.Messages.Where(x => x.Value.id == req.MessageId).First();

            var rewardRecord = GameData.Instance.GetRewardTableEntry(conversation.Value.reward_id) ?? throw new Exception("unable to lookup reward");

            user.SetSubQuest(req.SubQuestId, true);

            var conversationRecordUser = user.MessengerData.Where(x => x.MessageId == req.MessageId).First();
            conversationRecordUser.State = 2; // mark as claimed

            response.Reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}

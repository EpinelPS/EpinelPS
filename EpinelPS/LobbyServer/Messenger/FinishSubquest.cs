using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Messenger
{
    [PacketPath("/messenger/finsubquest")]
    public class FinishSubquest : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFinSubQuest req = await ReadData<ReqFinSubQuest>();
            User user = GetUser();

            ResFinSubQuest response = new();

            KeyValuePair<int, SubQuestRecord> opener = GameData.Instance.Subquests.Where(x => x.Key == req.SubQuestId).First();
            KeyValuePair<string, MessengerDialogRecord> conversation = GameData.Instance.Messages.Where(x => x.Value.Id == req.MessageId).First();

            RewardRecord rewardRecord = GameData.Instance.GetRewardTableEntry(conversation.Value.RewardId) ?? throw new Exception("unable to lookup reward");

            user.SetSubQuest(req.SubQuestId, true);

            NetMessage conversationRecordUser = user.MessengerData.Where(x => x.MessageId == req.MessageId).First();
            conversationRecordUser.State = 2; // mark as claimed

            response.Reward = RewardUtils.RegisterRewardsForUser(user, rewardRecord);
            JsonDb.Save();
            
            await WriteDataAsync(response);
        }
    }
}

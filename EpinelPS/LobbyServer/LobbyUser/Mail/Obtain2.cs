using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser.Mail;

[GameRequest("/mail/obtain2")]
public class Obtain2 : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqObtainMail2 req = await ReadData<ReqObtainMail2>();
        User user = GetUser();
        ResObtainMail2 response = new();
        NetRewardData ret = new();

        long timenow = DateTime.Now.Ticks;
        if (user.MailDatas.TryGetValue(req.Msn,out NetUserMailData? mailData))
        {

            foreach (var item in mailData.Items)
            {
                if (item.ExpiredAt >= timenow)
                {
                    RewardUtils.AddSingleObject(user, ref ret, item.RewardId, (RewardType)item.RewardType, item.RewardValue);
                }

            }
            mailData.State = 2;

            user.Badges.RemoveAll(b => b.BadgeContent == BadgeContents.MailboxMessage && b.Location == mailData.Msn.ToString());
            if (!user.MailDatas.Values.Any(m => m.State == 1 && m.HasReward))
            {
                user.Badges.RemoveAll(b => b.BadgeContent == BadgeContents.Mailbox || b.BadgeContent == BadgeContents.MailboxMessage);
            }

            response.Data = mailData;
            response.Result = ObtainMailResult.Success;
            response.Reward = ret;
        }


        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
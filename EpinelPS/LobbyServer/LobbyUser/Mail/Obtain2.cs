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

            response.Data = mailData;
            response.Result = ObtainMailResult.Success;
            response.Reward = ret;
        }


        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
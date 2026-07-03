using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Org.BouncyCastle.Asn1.Pkcs;

namespace EpinelPS.LobbyServer.LobbyUser.Mail;

[GameRequest("/mail/allobtain")]
public class AllObtain : LobbyMessage
{
    protected override async Task HandleAsync()
    {
        ReqAllObtainMail req = await ReadData<ReqAllObtainMail>();
        User user = GetUser();
        ResAllObtainMail response = new();
        NetRewardData ret = new();

        long timenow = DateTime.Now.Ticks;
        List<long>? keysToObtain = user.MailDatas.Where(x => x.Value.State == 0).Select(x => x.Key).ToList();

        foreach (var key in keysToObtain)
        {
            NetUserMailData? maildata = user.MailDatas[key];

            maildata.State = 2;
            response.SucceedMails.Add(new EpinelPS.Mail() { Msn = key,Type = maildata.Type});

            if (maildata.HasReward)
            {
                foreach (var item in maildata.Items)
                {
                    if (item.ExpiredAt >= timenow)
                    {
                        RewardUtils.AddSingleObject(user, ref ret, item.RewardId, (RewardType)item.RewardType, item.RewardValue);
                    }
                }
            }

        }

        response.Reward = ret;
        
        // TODO
        JsonDb.Save();
        await WriteDataAsync(response);
    }
}
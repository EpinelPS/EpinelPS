using EpinelPS.Data;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/mail/get")]
    public class GetMail : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetMailData req = await ReadData<ReqGetMailData>();

            ResGetMailData r = new();
            r.Mail.Add(new NetUserMailData()
            {

                Msn = 3,
                Nickname = "nick",
                Title = new() { IsPlain = true, Str = "Our Server Fell" },
                Text = new() { IsPlain = true, Str = "Our Game Was Down For 1 Second because the HP Laptop Which Hosted the Server Got Shut Down by the Lid Closing. As for the Reward for the Inconvience, free paid gems" },
                HasReward = true,
                Sender = 102

            });

            r.Mail[0].Items.Add(new NetMailRewardItem() { ExpiredAt = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(), RewardId = 1, RewardType = (int)CurrencyType.ChargeCash, RewardValue = 100000 });

            await WriteDataAsync(r);
        }
    }
}
